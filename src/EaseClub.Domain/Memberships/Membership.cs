using EaseClub.Domain.ApplicationTemplates;
using EaseClub.Domain.ApplicationTemplates.SystemSections;
using EaseClub.Domain.Clubs;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Member;
using EaseClub.Domain.MembershipApplications;
using EaseClub.Domain.MembershipApplications.Enums;
using EaseClub.Domain.MembershipApplications.ValueObjects;
using EaseClub.Domain.MembershipPlans;
using EaseClub.Domain.Memberships.Errors;
using EaseClub.Domain.Memberships.Events;
using EaseClub.Domain.Memberships.ValueObjects;
using EaseClub.Domain.MembershipTypes;
using System.Linq;

namespace EaseClub.Domain.Memberships
{
    public class Membership : AuditableEntity, IBelongToMember, IHaveClub
    {
        private Membership() { } // EF Core

        private Membership(
            Guid id,
            Guid memberId,
            Guid clubId,
            Guid membershipTypeId,
            Guid membershipPlanId,
            string membershipNumber,
            string? extraDataJson = null) : base(id)
        {
            MemberId = memberId;
            ClubId = clubId;
            MembershipTypeId = membershipTypeId;
            MembershipPlanId = membershipPlanId;
            MembershipNumber = membershipNumber;
            Status = MembershipStatus.Active;
            ExtraDataJson = extraDataJson;
        }

        // Properties
        public Guid MemberId { get; private set; }
        public Guid ClubId { get; private set; }
        public Club Club { get; private set; }
        public MemberUser Member { get; private set; }
        public Guid MembershipTypeId { get; private set; }
        public MembershipType MembershipType { get; private set; }
        public Guid MembershipPlanId { get; private set; }
        public MembershipPlan MembershipPlan { get; private set; }
        public MembershipStatus Status { get; private set; }
        public Guid? MembershipApplicationId { get; private set; } = Guid.Empty;

        private readonly List<MembershipCycle> _MembershipCycles = new List<MembershipCycle>();
        public IReadOnlyList<MembershipCycle> MembershipCycles => _MembershipCycles.AsReadOnly();
        public string MembershipNumber { get; private set; }
        public string? ExtraDataJson { get; private set; }

        // Family members
        private readonly List<FamilyMember> _FamilyMembers = new();
        public IReadOnlyList<FamilyMember> FamilyMembers => _FamilyMembers.AsReadOnly();

        //public int FamilyMemberCount => _FamilyMembers.Count;

        // Cancellation tracking
        public DateTime? CancelledAt { get; private set; }
        public string? CancellationReason { get; private set; }

        #region Factory Methods

        public static Result<Membership> CreateFromEnrollment(Enrollment enrollment, string membershipNumber, int maxFamilyMembers, MembershipApplication? application = null)
        {
            if (enrollment.Status != EnrollmentStatus.Completed)
                return Error.Conflict(description: "Enrollment is not completed.");

            if (enrollment.Source == EnrollmentSource.Renewal)
                return Error.Conflict(description: "Renewal enrollments should be applied to existing memberships.");

            var membership = new Membership(
                Guid.NewGuid(),
                enrollment.MemberId,
                enrollment.ClubId,
                enrollment.MembershipTypeId,
                enrollment.MembershipPlanId,
                membershipNumber);

            membership.MembershipApplicationId = enrollment.MembershipApplicationId;

            // Map family members if application is provided
            if (application != null)
            {
                var mapResult = MapFamilyMembers(membership, application, maxFamilyMembers);
                if (mapResult.IsError) return mapResult.TopError;
            }

            var start = DateTime.UtcNow;
            var end = start.AddYears(enrollment.SubscriptionValidityInYears);

            var rules = InstallmentDto.ToInstallments(enrollment.GetInstallments().ToList());
            if (rules.IsError) return rules.TopError;

            var cycleResult = MembershipCycle.Create(
                membership.Id,
                membership.ClubId,
                membership.MembershipTypeId,
                membership.MembershipPlanId,
                start,
                end,
                enrollment.TotalPrice,
                enrollment.InstallmentTemplateId,
                rules.Value);

            if (cycleResult.IsError)
                return cycleResult.TopError;

            membership._MembershipCycles.Add(cycleResult.Value);
            return membership;
        }

        #endregion

        private static Result<Success> MapFamilyMembers(Membership membership, MembershipApplication app, int maxAllowed)
        {
            var familySection = app.TemplateSnapshot.Steps
                .SelectMany(s => s.Sections)
                .FirstOrDefault(sec => sec.Intent == SectionIntent.FamilyMembers);

            if (familySection == null) return Result.Success;

            var groupedMembers = app.Answers
                .Where(a => familySection.Fields.Any(f => f.Id == a.FieldDefinitionId))
                .GroupBy(a => a.InstanceId);

            foreach (var group in groupedMembers)
            {
                if (membership._FamilyMembers.Count >= maxAllowed)
                    break; // Capacity reached

                var fullName = group.FirstOrDefault(a => a.FieldKey == FamilyMemberField.FullName)?.Value;
                var relationshipStr = group.FirstOrDefault(a => a.FieldKey == FamilyMemberField.Relationship)?.Value;
                var dobStr = group.FirstOrDefault(a => a.FieldKey == FamilyMemberField.DateOfBirth)?.Value;

                var dob = DateOnly.MinValue;
                if (DateOnly.TryParse(dobStr, out var parsedDob))
                {
                    dob = parsedDob;
                }
                else if (DateTime.TryParse(dobStr, out var parsedDateTime))
                {
                    dob = DateOnly.FromDateTime(parsedDateTime);
                }
                var rel  = Enum.TryParse<FamilyRelationship>(relationshipStr, true, out var parsedRel) ? parsedRel : default;

                var member = FamilyMember.Create(membership.Id,fullName, rel, dob);
                if(member.IsError) return member.TopError;

                 membership._FamilyMembers.Add(member.Value);
            }
            return Result.Success;
        }

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
                MemberId,
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
                MemberId,
                reason,
                DateTime.UtcNow));

            return Result.Success;
        }

        public Result<Success> Expire()
        {
            if(!GetCurrentCycle().Period.IsExpired(DateTime.UtcNow))
                return MembershipErrors.CannotPeriodOfCurrentCycleNotEnded;

            if (Status == MembershipStatus.Cancelled)
                return Error.Validation("Membership.Cannot.Expire.Cancelled",
                    "Cannot expire a cancelled membership.");

            Status = MembershipStatus.Expired;

            RaiseDomainEvent(new MembershipExpiredDomainEvent(
                Id,
                MemberId,
                DateTime.UtcNow));
            return Result.Success;
        }

        public Result<Success> ApplyRenewalFromEnrollment(Enrollment enrollment, int newPlanMaxFamilyMembers)
        {
            if (enrollment.Status != EnrollmentStatus.Completed)
                return Error.Conflict(description: "Only completed enrollments can be applied.");

            if (enrollment.Source != EnrollmentSource.Renewal)
                return Error.Conflict(description: "Only renewal enrollments can be applied to existing memberships.");

            if (enrollment.ExistingMembershipId != Id)
                return Error.Conflict(description: "Enrollment belongs to a different membership.");

            if (Status != MembershipStatus.Active && Status != MembershipStatus.Expired)
                return Error.Validation("Membership.Cannot.Renew", "Only active or expired memberships can be renewed.");

            var currentCycle = GetCurrentCycle();
            var oldEndDate = currentCycle?.Period.EndDate ?? DateTime.UtcNow;
            var newStartDate = oldEndDate.AddSeconds(1) > DateTime.UtcNow ? oldEndDate.AddSeconds(1) : DateTime.UtcNow;
            var newEndDate = newStartDate.AddYears(enrollment.SubscriptionValidityInYears);

            var rules = InstallmentDto.ToInstallments(enrollment.GetInstallments().ToList());
            if (rules.IsError) return rules.TopError;

            var cycleResult = MembershipCycle.Create(
                Id,
                ClubId,
                enrollment.MembershipTypeId,
                enrollment.MembershipPlanId,
                newStartDate,
                newEndDate,
                enrollment.TotalPrice,
                enrollment.InstallmentTemplateId,
                rules.Value);

            if (cycleResult.IsError)
                return cycleResult.TopError;

            _MembershipCycles.Add(cycleResult.Value);

            if (Status == MembershipStatus.Expired)
                Status = MembershipStatus.Active;

            if (MembershipPlanId != enrollment.MembershipPlanId)
            {
                // Validation: Check if new plan capacity fits existing family members
                if (newPlanMaxFamilyMembers < _FamilyMembers.Count)
                    return Error.Conflict("Membership.Renewal.CapacityExceeded", 
                        $"Cannot renew to this plan. It only allows {newPlanMaxFamilyMembers} family members, but you have {_FamilyMembers.Count}.");

                MembershipPlanId = enrollment.MembershipPlanId;
                MembershipTypeId = enrollment.MembershipTypeId;
            }

            RaiseDomainEvent(new MembershipRenewedDomainEvent(
                Id,
                MemberId,
                oldEndDate,
                newEndDate,
                enrollment.Id,
                DateTime.UtcNow));

            return Result.Success;
        }

        public Result<Success> Upgrade(Guid newMembershipTypeId, Guid newPlanId, int newPlanMaxFamilyMembers)
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

            // Validation: Check if new plan capacity fits existing family members
            if (newPlanMaxFamilyMembers < _FamilyMembers.Count)
                return Error.Conflict("Membership.Upgrade.CapacityExceeded",
                    $"Cannot upgrade to this plan. It only allows {newPlanMaxFamilyMembers} family members, but you have {_FamilyMembers.Count}.");

            var oldTypeId = MembershipTypeId;
            var oldPlanId = MembershipPlanId;

            MembershipTypeId = newMembershipTypeId;
            MembershipPlanId = newPlanId;

            RaiseDomainEvent(new MembershipUpgradedDomainEvent(
                Id,
                MemberId,
                oldTypeId,
                newMembershipTypeId,
                oldPlanId,
                newPlanId,
                DateTime.UtcNow));
            return Result.Success;
        }

        public Result<Success> Downgrade(Guid newMembershipTypeId, Guid newPlanId, int newPlanMaxFamilyMembers)
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

            // Validation: Check if new plan capacity fits existing family members
            if (newPlanMaxFamilyMembers < _FamilyMembers.Count)
                return Error.Conflict("Membership.Downgrade.CapacityExceeded",
                    $"Cannot downgrade to this plan. It only allows {newPlanMaxFamilyMembers} family members, but you have {_FamilyMembers.Count}.");

            var oldTypeId = MembershipTypeId;
            var oldPlanId = MembershipPlanId;

            MembershipTypeId = newMembershipTypeId;
            MembershipPlanId = newPlanId;

            RaiseDomainEvent(new MembershipDowngradedDomainEvent(
                Id,
                MemberId,
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

        public IReadOnlyList<MembershipCycle> GetMembershipCycles() => _MembershipCycles.AsReadOnly();

        public MembershipCycle? GetCurrentCycle()
        {
            var now = DateTime.UtcNow;

            var active = _MembershipCycles.FirstOrDefault(c => c.Period.IsActive(now));
            if (active != null) return active;

            // 2. If none active, find the one starting soonest (Future)
            var future = _MembershipCycles
                .Where(c => c.Period.IsFuture(now))
                .OrderBy(c => c.Period.StartDate)
                .FirstOrDefault();
            if (future != null) return future;

            // 3. Fallback: The most recently expired cycle
            return _MembershipCycles
                .OrderByDescending(c => c.Period.EndDate)
                .FirstOrDefault();
        }

        public bool IsActive()
        {
            return Status == MembershipStatus.Active &&
                   GetCurrentCycle()?.Period.IsActive(DateTime.UtcNow) == true;
        }

        public int DaysRemaining()
        {
            if (Status == MembershipStatus.Expired || Status == MembershipStatus.Cancelled)
                return 0;

            return (GetCurrentCycle()?.Period.EndDate - DateTime.UtcNow)?.Days ?? 0;
        }

        public DateTime FarestEndDate => _MembershipCycles.Any()
            ? _MembershipCycles.Max(c => c.Period.EndDate)
            : (GetCurrentCycle()?.Period.EndDate ?? CreatedAt);

        #endregion
    }
}
