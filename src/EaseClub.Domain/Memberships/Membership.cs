using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Membership;
using EaseClub.Domain.Memberships.Errors;
using EaseClub.Domain.Memberships.Events;
using EaseClub.Domain.Memberships.ValueObjects;

namespace EaseClub.Domain.Memberships
{
    public class Membership : AuditableEntity
    {
        private readonly List<FamilyMemberInfo> _familyMembers = new();

        private Membership() { } // EF Core

        private Membership(
            Guid id,
            Guid userId,
            Guid clubId,
            Guid membershipTypeId,
            Guid membershipPlanId,
            MembershipPeriod period,
            string? extraDataJson = null,
            string? primaryContact = null) : base(id)
        {
            UserId = userId;
            ClubId = clubId;
            MembershipTypeId = membershipTypeId;
            MembershipPlanId = membershipPlanId;
            Period = period;
            Status = MembershipStatus.Active;
            ExtraDataJson = extraDataJson;
            PrimaryContact = primaryContact;
        }

        // Properties
        public Guid UserId { get; private set; }
        public Guid ClubId { get; private set; }
        public Guid MembershipTypeId { get; private set; }
        public Guid MembershipPlanId { get; private set; }
        public MembershipPeriod Period { get; private set; }
        public MembershipStatus Status { get; private set; }

        // Optional fields persisted from application
        public string? PrimaryContact { get; private set; }
        public string? ExtraDataJson { get; private set; }

        // Family members
        public IReadOnlyList<FamilyMemberInfo> FamilyMembers => _familyMembers.AsReadOnly();
        public int FamilyMemberCount => _familyMembers.Count;

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
                extraDataJson,
                primaryContact);

            membership.RaiseDomainEvent(new MembershipCreatedDomainEvent(
                membership.Id,
                userId,
                clubId,
                membershipTypeId,
                membershipPlanId,
                startDate,
                endDate,
                DateTime.UtcNow));

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
                DateTime.UtcNow,
                DateTime.UtcNow));

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
                DateTime.UtcNow,
                DateTime.UtcNow));

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
                DateTime.UtcNow,
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
                DateTime.UtcNow,
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
                DateTime.UtcNow,
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
                DateTime.UtcNow,
                DateTime.UtcNow));

            return Result.Success;
        }

        #endregion

        #region Family Members Management

        public Result<Success> AddFamilyMember(
            Guid familyMemberId,
            string fullName,
            string relationship,
            DateTime dateOfBirth,
            int maxAllowed)
        {
            if (Status != MembershipStatus.Active)
                return MembershipErrors.NotActive;

            if (_familyMembers.Count >= maxAllowed)
                return MembershipErrors.MaxFamilyMembersReached;

            if (_familyMembers.Any(f => f.FamilyMemberId == familyMemberId))
                return Error.Conflict("Membership.FamilyMember.AlreadyExists",
                    "This family member is already added.");

            var familyMemberResult = FamilyMemberInfo.Create(
                familyMemberId,
                fullName,
                relationship,
                dateOfBirth);

            if (familyMemberResult.IsError)
                return familyMemberResult.TopError;

            _familyMembers.Add(familyMemberResult.Value);

            RaiseDomainEvent(new FamilyMemberAddedDomainEvent(
                Id,
                familyMemberId,
                fullName,
                DateTime.UtcNow,
                DateTime.UtcNow));

            return Result.Success;
        }

        public Result<Success> RemoveFamilyMember(Guid familyMemberId)
        {
            var familyMember = _familyMembers.FirstOrDefault(f => f.FamilyMemberId == familyMemberId);

            if (familyMember == null)
                return MembershipErrors.FamilyMemberNotFound;

            _familyMembers.Remove(familyMember);

            RaiseDomainEvent(new FamilyMemberRemovedDomainEvent(
                Id,
                familyMemberId,
                familyMember.FullName,
                DateTime.UtcNow,
                DateTime.UtcNow));

            return Result.Success;
        }

        #endregion

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

        public bool HasFamilyMembers()
        {
            return _familyMembers.Any();
        }

        #endregion
    }
}