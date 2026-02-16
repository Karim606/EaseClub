using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipApplications.Enums;
using EaseClub.Domain.MembershipApplications.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.MembershipApplications
{
    public class MembershipApplication:AuditableEntity
    {
        private MembershipApplication() { }

        private MembershipApplication(
            Guid id,
            string trackingNumber,
            Guid userId,
            Guid clubId,
            Guid membershipTypeId,
            Guid membershipPlanId,
            Guid templateId,
            decimal basePrice)
            : base(id)
        {
            TrackingNumber = trackingNumber;
            UserId = userId;
            ClubId = clubId;
            MembershipTypeId = membershipTypeId;
            MembershipPlanId = membershipPlanId;
            TemplateId = templateId;
            BasePrice = basePrice;
            FinalPrice = basePrice;
            Status = ApplicationStatus.Draft;
            PricingState = PricingState.Estimated;
            CreatedAt = DateTime.UtcNow;
        }
        public string TrackingNumber { get; private set; }
        public Guid UserId { get; private set; }
        public Guid ClubId { get; private set; }
        public Guid MembershipTypeId { get; private set; }
        public Guid MembershipPlanId { get; private set; }
        public Guid TemplateId { get; private set; }// frozen template
        public ApplicationStatus Status { get; private set; }// Draft, Submitted, Paid
        public decimal BasePrice { get; private set; }
        public decimal FinalPrice { get; private set; }
        public PricingState PricingState { get; private set; }// Estimated / Locked
        public DateTime CreatedAt { get; private set; }
        public DateTime? SubmittedAt { get; private set; }

        private readonly List<ApplicationStepInstance> _Steps = new();
        public IReadOnlyList<ApplicationStepInstance> Steps => _Steps.AsReadOnly();

        // =========================
        // Factory
        // =========================

        public static Result<MembershipApplication> Create(
            Guid id,
            string trackingNumber,
            Guid userId,
            Guid clubId,
            Guid membershipTypeId,
            Guid membershipPlanId,
            Guid templateId,
            decimal basePrice)
        {
            if (string.IsNullOrWhiteSpace(trackingNumber))
                return MembershipApplicationErrors.TrackingNumberRequired;

            if (userId == Guid.Empty)
                return MembershipApplicationErrors.UserIdRequired;

            if (clubId == Guid.Empty)
                return MembershipApplicationErrors.ClubIdRequired;

            if (basePrice < 0)
                return MembershipApplicationErrors.InvalidBasePrice;

            return new MembershipApplication(
                id,
                trackingNumber,
                userId,
                clubId,
                membershipTypeId,
                membershipPlanId,
                templateId,
                basePrice);
        }

        // =========================
        // Business Methods
        // =========================

        public Result<ApplicationStepInstance> AddNewStepInstance(Guid templateStepId)
        {
            if (Status != ApplicationStatus.Draft)
                return MembershipApplicationErrors.CantModifyNonDraft;

            if (_Steps.Any(s => s.TemplateStepId == templateStepId))
                return MembershipApplicationErrors.StepAlreadyExists;

            // Call internal factory
            var stepResult = ApplicationStepInstance.Create(Guid.NewGuid(), this.Id, templateStepId);

            if (stepResult.IsError) return stepResult.TopError;

            _Steps.Add(stepResult.Value);
            return stepResult.Value;
        }

        public Result<Success> Submit()
        {
            if (Status != ApplicationStatus.Draft)
                return MembershipApplicationErrors.InvalidStatusTransition;

            Status = ApplicationStatus.Submitted;
            SubmittedAt = DateTime.UtcNow;
            PricingState = PricingState.Locked;

            return Result.Success;
        }

        public Result<Success> ApplyPricing(decimal newPrice)
        {
            if (PricingState == PricingState.Locked)
                return MembershipApplicationErrors.PricingLocked; 

            FinalPrice = newPrice;
            return Result.Success;
        }

    }
}
