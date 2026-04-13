using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipApplications;
using EaseClub.Domain.MembershipApplications.Enums;
using EaseClub.Domain.MembershipApplications.ValueObjects;
using EaseClub.Domain.MembershipPlans;
using EaseClub.Domain.Payment;
using EaseClub.Domain.Payment.Enums;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Memberships
{
    public enum PendingEnrollmentSource
    {
        DirectPay = 1,
        ApplicationApproval = 2,
        Renewal = 3
    }

    public enum PendingEnrollmentStatus
    {
        WaitingForFirstPayment = 1,
        Completed = 2,
        Expired = 3,
        Cancelled = 4
    }


    public class PendingEnrollment :AuditableEntity,IBillingItem
    {
        private const int DefaultExpiryHours = 48;

        private PendingEnrollment() { }

        private PendingEnrollment(
            Guid id,
            Guid userId,
            Guid clubId,
            Guid membershipTypeId,
            Guid membershipPlanId,
            Guid? membershipApplicationId,
            Guid? installmentTemplateId,
            string installmentsJson,
            decimal totalPrice,
            int subscriptionValidityInYears,
            decimal firstInstallmentAmount,
            DateTime expiresAt,
            PendingEnrollmentSource source,
            Guid? existingMembershipId) : base(id)
        {
            UserId = userId;
            ClubId = clubId;
            MembershipTypeId = membershipTypeId;
            MembershipPlanId = membershipPlanId;
            MembershipApplicationId = membershipApplicationId;
            InstallmentTemplateId = installmentTemplateId;
            InstallmentsJson = installmentsJson;
            TotalPrice = totalPrice;
            SubscriptionValidityInYears = subscriptionValidityInYears;
            Amount = firstInstallmentAmount;
            ExpiresAt = expiresAt;
            Source = source;
            Status = PendingEnrollmentStatus.WaitingForFirstPayment;
            ReadableId = $"PEN-{DateTime.UtcNow:yyyyMMdd}-{id.ToString()[..8].ToUpper()}";
            ExistingMembershipId = existingMembershipId;
        }

        public Guid UserId { get; private set; }
        public Guid ClubId { get; private set; }
        public Guid MembershipTypeId { get; private set; }
        public Guid MembershipPlanId { get; private set; }
        public Guid? MembershipApplicationId { get; private set; }
        public Guid? InstallmentTemplateId { get; private set; }
        public decimal TotalPrice { get; private set; }
        public int SubscriptionValidityInYears { get; private set; }
        public decimal Amount { get; private set; }
        public string ReadableId { get; private set; } = string.Empty;
        public Guid? ExistingMembershipId { get; private set; }
        public Guid? FirstInvoiceId { get; private set; }
        //public DateTime FirstInvoiceDueDate { get; private set; }
        public DateTime ExpiresAt { get; private set; }
        public string InstallmentsJson { get; private set; } = "[]";
        public PendingEnrollmentSource Source { get; private set; }
        public PendingEnrollmentStatus Status { get; private set; }

        public BillingItemType GetBillingType()
            => BillingItemType.PendingEnrollmentFirstInstallment;

        public static Result<PendingEnrollment> CreateForDirectPay(
            Guid userId,
            Guid clubId,
            MembershipPlan plan,
            InstallmentTemplate? installmentTemplate)
        {
            if (plan.EnrollmentMode != EnrollmentMode.DirectPay)
                return Error.Conflict(description: "Plan WrongEnrollmentMode");

            var req = new EnrollmentRequest(userId,
                clubId,
                plan,
                installmentTemplate,
                null,
                null,
                PendingEnrollmentSource.ApplicationApproval,
                plan.SubscriptionValidityInYears,
                plan.TotalPrice);


            return Create(req);
        }

        public static Result<PendingEnrollment> CreateFromApprovedApplication(
            MembershipApplication app,
            MembershipPlan plan)
        {
            if (app.Status != ApplicationStatus.Approved)
                return Error.Conflict(description: "Only approved applications can start payment.");

            if (app.FinalPriceSummary == null)
                return Error.Conflict(description: "Application final price is missing.");

            if (app.MembershipPlanId != plan.Id)
                return Error.Conflict(description: "current plan isnt same plan exists in application");

            InstallmentTemplate? template = null;
            if( app.InstallmentTemplateId != null)
            {
                var instRules = InstallmentRuleSnapshot.ListToDomain(app.TemplateSnapshot.InstallmentRules);
                if (instRules.IsError) return instRules.TopError;

                var res = InstallmentTemplate.Create(app.InstallmentTemplateId.Value, app.ClubId, "temp",null,null,instRules.Value);
                if (res.IsError) return res.TopError;

                template = res.Value;
            }

            var req = new EnrollmentRequest(app.UserId,
                app.ClubId,
                plan,
                template,
                app.Id,
                null,
                PendingEnrollmentSource.ApplicationApproval,
                plan.SubscriptionValidityInYears,
                app.FinalPriceSummary.TotalPrice);
            

            return Create(req);
        }

        public static Result<PendingEnrollment> CreateForRenewal(
    Membership membership,
    MembershipPlan plan,
    InstallmentTemplate? installmentTemplate)
        {
            // Validate membership can be renewed
            if (membership.Status != MembershipStatus.Active
                && membership.Status != MembershipStatus.Expired)
                return Error.Conflict("Renewal.InvalidStatus",
                    "Only active or expired memberships can be renewed.");
            if(installmentTemplate != null && plan.InstallmentsAllowdInRenewal == false)
                return Error.Conflict("Renewal.InstallmentsNotAllowed",
                    "This plan does not allow installments in renewal.");

            var req = new EnrollmentRequest(
            membership.UserId,
            membership.ClubId,
            plan,
            installmentTemplate,
            null,
            membership.Id,
            PendingEnrollmentSource.Renewal,
            plan.SubscriptionValidityInYears,
            plan.TotalPrice);

            return Create(req);
        }

        private static Result<PendingEnrollment> Create(EnrollmentRequest req)
        {
            if (req.UserId == Guid.Empty || req.ClubId == Guid.Empty)
                return Error.Validation("InvalidReferences");

            if (req.Template != null && !req.Plan.SupportsTemplate(req.Template.Id))
                return Error.Conflict("InvalidTemplate");

            if (req.ApplicationId != null && req.ExistingMembershipId != null)
                return Error.Conflict(description: "Linking existing membership and application is not allowed, linking application for creation of membership , linking membershipId for renewal only.");

            var rules = req.Template.Installments.ToList()
                ?? new List<Installment>
                {
                    Installment.Create(100m, 0, 1).Value
                };
            var generatedInstallments = InstallmentEngine.GenerateMembershipInstallments(rules, req.TotalPrice);
            if (generatedInstallments.IsError)
                return generatedInstallments.TopError;


            var firstAmount = generatedInstallments.Value.First().Amount;
            var dueDate = DateTime.UtcNow;

            return new PendingEnrollment(
            Guid.NewGuid(),
            req.UserId,
            req.ClubId,
            req.Plan.MembershipTypeId,
            req.Plan.Id,
            req.ApplicationId,
            req.Template?.Id,
            System.Text.Json.JsonSerializer.Serialize(generatedInstallments),
            req.TotalPrice,
            req.ValidityYears,
            firstAmount,
            dueDate.AddHours(DefaultExpiryHours),
            req.Source,
            req.ExistingMembershipId
    );
        }


        public IReadOnlyList<InstallmentDto> GetInstallments()
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<InstallmentDto>>(InstallmentsJson)
                ?? new List<InstallmentDto>();
        }

        public Result<Success> AttachFirstInvoice(Guid invoiceId)
        {
            if (invoiceId == Guid.Empty)
                return Error.Validation(description: "PendingEnrollment.FirstInvoiceIdRequired");

            if (FirstInvoiceId == invoiceId)
                return Result.Success;

            if (FirstInvoiceId.HasValue && FirstInvoiceId.Value != invoiceId)
                return Error.Conflict(description: "PendingEnrollment.FirstInvoiceAlreadyAttached");

            FirstInvoiceId = invoiceId;
            return Result.Success;
        }

        public bool IsExpired(DateTime nowUtc) => Status == PendingEnrollmentStatus.Expired || nowUtc > ExpiresAt;

        public Result<Success> MarkCompleted()
        {
            if (Status == PendingEnrollmentStatus.Completed)
                return Result.Success;

            if (IsExpired(DateTime.UtcNow))
                return Error.Conflict(description: "PendingEnrollment.Expired");

            Status = PendingEnrollmentStatus.Completed;
            return Result.Success;
        }

        public void MarkExpired()
        {
            Status = PendingEnrollmentStatus.Expired;
        }

        private sealed record EnrollmentRequest(
        Guid UserId,
        Guid ClubId,
        MembershipPlan Plan,
        InstallmentTemplate? Template,
        Guid? ApplicationId,
        Guid? ExistingMembershipId,
        PendingEnrollmentSource Source,
        int ValidityYears,
        decimal TotalPrice);

    }

    public sealed record InstallmentDto(decimal PercentageOfAmount, int DueAfterDays, int OrderIndex)
    {
        public static Result<List<Installment>> ToInstallments(List<InstallmentDto> list)
        {
            var installments = new List<Installment>();

            foreach (var item in list)
            {
                var result = Installment.Create(item.PercentageOfAmount, item.DueAfterDays, item.OrderIndex);

                if (result.IsError)
                    return result.TopError; // Return the specific domain error encountered

                installments.Add(result.Value);
            }

            return installments;
        }
    }
}

