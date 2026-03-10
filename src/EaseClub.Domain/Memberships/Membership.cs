using EaseClub.Domain.Clubs;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Membership;
using EaseClub.Domain.MembershipApplications;
using EaseClub.Domain.MembershipApplications.ValueObjects;
using EaseClub.Domain.MembershipPlans;
using EaseClub.Domain.Memberships.Errors;
using EaseClub.Domain.Memberships.Events;
using EaseClub.Domain.MembershipApplications.Enums;
using EaseClub.Domain.Memberships.ValueObjects;
using EaseClub.Domain.MembershipTypes;

namespace EaseClub.Domain.Memberships
{
    public class Membership : AuditableEntity
    {

        private Membership() { } // EF Core

        private Membership(
            Guid id,
            Guid userId,
            Guid clubId,
            Guid membershipTypeId,
            Guid membershipPlanId,
            MembershipPeriod period,
            string? extraDataJson = null)  : base(id)
        {
            UserId = userId;
            ClubId = clubId;
            MembershipTypeId = membershipTypeId;
            MembershipPlanId = membershipPlanId;
            Period = period;
            Status = MembershipStatus.Active;
            ExtraDataJson = extraDataJson;
        }

        // Properties
        public Guid UserId { get; private set; }
        public Guid ClubId { get; private set; }
        public Guid MembershipTypeId { get; private set; }
        public MembershipType MembershipType { get; private set; }
        public Guid MembershipPlanId { get; private set; }
        public MembershipPlan MembershipPlan { get; private set; }
        public MembershipPeriod Period { get; private set; }
        public MembershipStatus Status { get; private set; }

        public string? ExtraDataJson { get; private set; }

        // Family members
        //private readonly List<FamilyMemberInfo> _FamilyMembers = new();
        //public IReadOnlyList<FamilyMemberInfo> FamilyMembers => _FamilyMembers.AsReadOnly();

        //public int FamilyMemberCount => _FamilyMembers.Count;

        private readonly List<MembershipInstallment> _MembershipInstallments = new();
        public IReadOnlyList<MembershipInstallment> MembershipInstallments => _MembershipInstallments.AsReadOnly();

        // Cancellation tracking
        public DateTime? CancelledAt { get; private set; }
        public string? CancellationReason { get; private set; }

        #region Factory Methods

        public static Result<Membership> Create(
            Guid id,
            Guid userId,
            Guid clubId,
            Guid membershipTypeId,
            Guid membershipPlanId,
            DateTime startDate,
            DateTime endDate,
            string? extraDataJson = null,
            string? primaryContact = null)
        {
            // Validation
            if (id == Guid.Empty)
                return Error.Validation("Membership.Id.Required", "Membership ID is required.");
            if (userId == Guid.Empty)
                return MembershipErrors.UserIdRequired;
            if (clubId == Guid.Empty)
                return MembershipErrors.ClubIdRequired;
            if (membershipTypeId == Guid.Empty)
                return MembershipErrors.MembershipTypeIdRequired;
            if (membershipPlanId == Guid.Empty)
                return MembershipErrors.MembershipPlanIdRequired;

            var periodResult = MembershipPeriod.Create(startDate, endDate);
            if (periodResult.IsError)
                return periodResult.TopError;

            var membership = new Membership(
                id,
                userId,
                clubId,
                membershipTypeId,
                membershipPlanId,
                periodResult.Value,
                extraDataJson
                );

            membership.RaiseDomainEvent(new MembershipCreatedDomainEvent(
                membership.Id,
                userId,
                clubId,
                membershipTypeId,
                membershipPlanId,
                startDate,
                endDate));

            return membership;
        }

        public static Result<Membership> CreateFromApplication(
            MembershipApplication app)
        {
            // Skip the plan.SupportsTemplate check because it was verified 
            // when the Application was submitted.
            if(app.Status!= ApplicationStatus.Approved) return Error.Conflict(description:"Membership.Cannot.CreateFromApplication.NotApproved");
            var subYears = app.TemplateSnapshot.MembershipPlan.SubscriptionValidityInYears;
            var membershipPeriod = MembershipPeriod.Create(DateTime.UtcNow, DateTime.UtcNow.AddYears(subYears));
            if (membershipPeriod.IsError) return membershipPeriod.TopError;

            var membership = new Membership(
                Guid.NewGuid(),
                app.UserId,
                app.ClubId,
                app.MembershipTypeId,
                app.MembershipPlanId,
                membershipPeriod.Value
                );

            var installmentRules = InstallmentRuleSnapshot.ListToDomain(app.TemplateSnapshot.InstallmentRules);

            if(installmentRules.IsError) return installmentRules.TopError;

            var instBluePrint = InstallmentEngine.GenerateMembershipInstallments(installmentRules.Value,app.FinalPriceSummary!.TotalPrice);

            if(instBluePrint.IsError) return instBluePrint.TopError;
            // Directly stamp the frozen installments from the snapshot
            foreach (var bp in instBluePrint.Value)
            {
                membership._MembershipInstallments.Add(MembershipInstallment.Create(
                    membership.Id,
                    app.ClubId,
                    app.MembershipTypeId,
                    app.MembershipPlanId,
                    app.TemplateId, // Use the ID from the frozen application
                    bp.Order,
                    bp.Amount,
                    bp.DueDate
                ).Value);
            }
            membership.Status = MembershipStatus.Suspended;
            return membership;
        }

        #endregion

        #region Lifecycle Commands

        public Result<Success> Activate()
        {
            if (Status == MembershipStatus.Active)
                return MembershipErrors.AlreadyActive;

            if (Status == MembershipStatus.Expired)
                return MembershipErrors.CannotActivateExpired;

            if (Status == MembershipStatus.Cancelled)
                return MembershipErrors.CannotActivateCancelled;

            Status = MembershipStatus.Active;

            RaiseDomainEvent(new MembershipActivatedDomainEvent(
                Id,
                UserId,
                DateTime.UtcNow
                ));

            return Result.Success;
        }

        public Result<Success> Cancel(string? reason = null)
        {
            if (Status == MembershipStatus.Cancelled)
                return MembershipErrors.AlreadyCancelled;

            if (Status == MembershipStatus.Expired)
                return MembershipErrors.AlreadyExpired;

            Status = MembershipStatus.Cancelled;
            CancelledAt = DateTime.UtcNow;
            CancellationReason = reason;

            RaiseDomainEvent(new MembershipCancelledDomainEvent(
                Id,
                UserId,
                reason,
                DateTime.UtcNow
                ));

            return Result.Success;
        }

        public Result<Success> Expire()
        {
            if (Status == MembershipStatus.Expired)
                return MembershipErrors.AlreadyExpired;

            if (Status == MembershipStatus.Cancelled)
                return Error.Validation("Membership.Cannot.Expire.Cancelled",
                    "Cannot expire a cancelled membership.");

            Status = MembershipStatus.Expired;

            RaiseDomainEvent(new MembershipExpiredDomainEvent(
                Id,
                UserId,
                DateTime.UtcNow));
            return Result.Success;
        }

        public Result<Success> Renew(DateTime newEndDate, Guid? newPlanId = null)
        {
            if (Status != MembershipStatus.Active && Status != MembershipStatus.Expired)
                return Error.Validation("Membership.Cannot.Renew",
                    "Only active or expired memberships can be renewed.");

            if (newEndDate <= Period.EndDate)
                return MembershipErrors.InvalidRenewalDate;

            var oldEndDate = Period.EndDate;
            var periodResult = MembershipPeriod.Create(Period.StartDate, newEndDate);

            if (periodResult.IsError)
                return periodResult.TopError;

            Period = periodResult.Value;

            if (newPlanId.HasValue)
                MembershipPlanId = newPlanId.Value;

            if (Status == MembershipStatus.Expired)
                Status = MembershipStatus.Active;

            RaiseDomainEvent(new MembershipRenewedDomainEvent(
                Id,
                UserId,
                oldEndDate,
                newEndDate,
                newPlanId,
                DateTime.UtcNow));

            return Result.Success;
        }

        public Result<Success> Upgrade(Guid newMembershipTypeId, Guid newPlanId)
        {
            if (Status != MembershipStatus.Active)
                return MembershipErrors.NotActive;

            if (newMembershipTypeId == Guid.Empty)
                return Error.Validation("Membership.NewTypeId.Required",
                    "New membership type ID is required.");

            if (newPlanId == Guid.Empty)
                return Error.Validation("Membership.NewPlanId.Required",
                    "New plan ID is required.");

            if (newMembershipTypeId == MembershipTypeId)
                return MembershipErrors.InvalidUpgrade;

            var oldTypeId = MembershipTypeId;
            var oldPlanId = MembershipPlanId;

            MembershipTypeId = newMembershipTypeId;
            MembershipPlanId = newPlanId;

            RaiseDomainEvent(new MembershipUpgradedDomainEvent(
                Id,
                UserId,
                oldTypeId,
                newMembershipTypeId,
                oldPlanId,
                newPlanId,
                DateTime.UtcNow));
            return Result.Success;
        }

        public Result<Success> Downgrade(Guid newMembershipTypeId, Guid newPlanId)
        {
            if (Status != MembershipStatus.Active)
                return MembershipErrors.NotActive;

            if (newMembershipTypeId == Guid.Empty)
                return Error.Validation("Membership.NewTypeId.Required",
                    "New membership type ID is required.");

            if (newPlanId == Guid.Empty)
                return Error.Validation("Membership.NewPlanId.Required",
                    "New plan ID is required.");

            if (newMembershipTypeId == MembershipTypeId)
                return MembershipErrors.InvalidDowngrade;

            var oldTypeId = MembershipTypeId;
            var oldPlanId = MembershipPlanId;

            MembershipTypeId = newMembershipTypeId;
            MembershipPlanId = newPlanId;

            RaiseDomainEvent(new MembershipDowngradedDomainEvent(
                Id,
                UserId,
                oldTypeId,
                newMembershipTypeId,
                oldPlanId,
                newPlanId,
                DateTime.UtcNow));

            return Result.Success;
        }

        #endregion

        //#region Family Members Management

        //public Result<Success> AddFamilyMember(
        //    Guid familyMemberId,
        //    string fullName,
        //    string relationship,
        //    DateTime dateOfBirth,
        //    int maxAllowed)
        //{
        //    if (Status != MembershipStatus.Active)
        //        return MembershipErrors.NotActive;

        //    if (_FamilyMembers.Count >= maxAllowed)
        //        return MembershipErrors.MaxFamilyMembersReached;

        //    if (_FamilyMembers.Any(f => f.FamilyMemberId == familyMemberId))
        //        return Error.Conflict("Membership.FamilyMember.AlreadyExists",
        //            "This family member is already added.");

        //    var familyMemberResult = FamilyMemberInfo.Create(
        //        familyMemberId,
        //        fullName,
        //        relationship,
        //        dateOfBirth);

        //    if (familyMemberResult.IsError)
        //        return familyMemberResult.TopError;

        //    _FamilyMembers.Add(familyMemberResult.Value);

        //    RaiseDomainEvent(new FamilyMemberAddedDomainEvent(
        //        Id,
        //        familyMemberId,
        //        fullName,
        //        DateTime.UtcNow));

        //    return Result.Success;
        //}

        //public Result<Success> RemoveFamilyMember(Guid familyMemberId)
        //{
        //    var familyMember = _FamilyMembers.FirstOrDefault(f => f.FamilyMemberId == familyMemberId);

        //    if (familyMember == null)
        //        return MembershipErrors.FamilyMemberNotFound;

        //    _FamilyMembers.Remove(familyMember);

        //    RaiseDomainEvent(new FamilyMemberRemovedDomainEvent(
        //        Id,
        //        familyMemberId,
        //        familyMember.FullName,
        //        DateTime.UtcNow));

        //    return Result.Success;
        //}

        //#endregion

        #region Query Methods

        public bool IsActive()
        {
            return Status == MembershipStatus.Active &&
                   Period.IsActive(DateTime.UtcNow);
        }

        public bool IsExpired()
        {
            return Status == MembershipStatus.Expired ||
                   Period.IsExpired(DateTime.UtcNow);
        }

        public bool IsCancelled()
        {
            return Status == MembershipStatus.Cancelled;
        }

        public int DaysRemaining()
        {
            if (IsExpired() || IsCancelled())
                return 0;

            return (Period.EndDate - DateTime.UtcNow).Days;
        }

        //public bool HasFamilyMembers()
        //{
        //    return _FamilyMembers.Any();
        //}

        #endregion


        public Result<Success> GenerateInstallments(InstallmentTemplate template,decimal totalPrice)
        {
            // 1. Verify the link between Plan and Template
            if (!MembershipPlan.SupportsTemplate(template.Id))
                return Error.Conflict(description:"Membership.Plan.DoesNotSupportTemplate");

            // 2. Calculate the installments using the current logic
            var result = InstallmentEngine.GenerateMembershipInstallments(template.Installments,totalPrice);
            if (result.IsError) return result.TopError;

            // 3. Clear existing PENDING installments 
            // (We keep the PAID ones for history!)
            var pending = _MembershipInstallments.Where(i => i.Status == InstallmentStatus.Pending).ToList();
            foreach (var item in pending) _MembershipInstallments.Remove(item);

            // 4. Add the new ones
            // IMPORTANT: Store the snapshot of the rule used
            _MembershipInstallments.AddRange(result.Value.Select(bp =>
                MembershipInstallment.Create(
                    this.Id,
                    ClubId,
                    MembershipTypeId,
                    MembershipPlan.Id,
                    template.Id, // Link to the definition
                    bp.Order,
                    bp.Amount,
                    bp.DueDate
                ).Value
            ));

            return Result.Success;
        }

    }
}