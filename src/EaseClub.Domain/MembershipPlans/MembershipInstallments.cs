using EaseClub.Domain.Clubs;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Memberships;
using EaseClub.Domain.Payment;
using EaseClub.Domain.Payment.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.MembershipPlans
{
    public class MembershipInstallment : AuditableEntity,IHaveClub,IBillingItem
    {
        public Guid MembershipCycleId { get; private set; }
        public Guid ClubId { get; private set; }
        public Guid MembershipTypeId { get; private set; }
        public Guid MembershipPlanId { get; private set; }
        public Guid? InstallmentTemplateId { get; private set; }
        public BillingItemType GetBillingType() => BillingItemType.MembershipInstallment;
        public MembershipCycle Membership { get; private set; }
        public Club Club { get; private set; }
        public int Order { get; private set; }
        public decimal Amount { get; private set; }
        public DateTime DueDate { get; private set; }
        public InstallmentStatus Status { get; private set; }
        public Guid? InvoiceId { get; private set; }
        public string ReadableId { get; private set; }
        private MembershipInstallment() { }

        public MembershipInstallment(Guid membershipCycleId,Guid clubId,Guid membershipTypeId,Guid planId,Guid? installmentTemplateId,
            int order, decimal amount, DateTime dueDate):base(Guid.NewGuid())
        {
            MembershipCycleId = membershipCycleId;
            ClubId = clubId;
            Order = order;
            Amount = amount;
            DueDate = dueDate;
            Status = InstallmentStatus.Pending;
            MembershipTypeId = membershipTypeId;
            MembershipPlanId = planId;
            InstallmentTemplateId = installmentTemplateId;
        }

        public Result<Success> MarkPaid(Guid invoiceId)
        {
            if (Status == InstallmentStatus.Pending || Status == InstallmentStatus.Overdue) { 
                Status = InstallmentStatus.Paid;
                InvoiceId = invoiceId;

                return Result.Success;
            }
            return MembershipInstallmentErrors.OnlyPendingInstallmentOrOverDueCanBePaid;
        }

        public Result<Success> MarkOverdue()
        {
            if(Status != InstallmentStatus.Pending)
                return MembershipInstallmentErrors.CantMarkUnPendingInstallmentAsOverDue;

            if (DueDate > DateTime.UtcNow)
                return MembershipInstallmentErrors.NotOverDuedYet;
            
            Status = InstallmentStatus.Overdue;
            return Result.Success;

        }

        public static Result<MembershipInstallment> Create(Guid membershipCycleId,Guid clubId,Guid membershipTypeId,
           Guid planId,Guid? installmentTemplateId, int order, decimal amount, DateTime dueDate)
        {
            if (membershipCycleId == Guid.Empty)
               return MembershipInstallmentErrors.MembershipCycleIdMustBeProvided;
            if (clubId == Guid.Empty)
                return OwnedByClubErrors.ClubIdIsRequired;

            if (membershipTypeId == Guid.Empty)
                return Error.Validation(description:"MembershipTypeIdMustBeProvided");

            if (planId == Guid.Empty)
                return Error.Validation(description: "MembershipTypeIdMustBeProvided");

            if (order < 0)
                return MembershipInstallmentErrors.InstallmentOrderMustBeNonNegative;
            
            if (amount <= 0)
                return  MembershipInstallmentErrors.InstallmentAmountMustBeGreaterThanZero;
           
            if (dueDate < DateTime.UtcNow.Date)
                return MembershipInstallmentErrors.InstallmentDueDateMustBeInTheFuture;

            var membershipInstallment = new MembershipInstallment(membershipCycleId, clubId, membershipTypeId, planId, installmentTemplateId, order, amount, dueDate);
             membershipInstallment.ReadableId = membershipInstallment.GetReadableInstallmentId();

            return membershipInstallment;
        }

        public string GetReadableInstallmentId()
        {
            return BillingITemIdGenerator.Generate(
                BillingItemType.MembershipInstallment,
                MembershipCycleId,
                ClubId,
                Id,  // installment guid
                DueDate);
        }
    }
}
