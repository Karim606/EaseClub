using EaseClub.Domain.ApplicationTemplates;
using EaseClub.Domain.ApplicationTemplates.SystemSections;
using EaseClub.Domain.Clubs;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipApplications;
using EaseClub.Domain.MembershipApplications.Enums;
using EaseClub.Domain.MembershipApplications.ValueObjects;
using EaseClub.Domain.MembershipPlans;
using EaseClub.Domain.Memberships.Errors;
using EaseClub.Domain.Memberships.Events;
using EaseClub.Domain.Memberships.ValueObjects;
using EaseClub.Domain.MembershipTypes;
using System.Linq;
using System.Numerics;

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
            string? extraDataJson = null)  : base(id)
        {
            UserId = userId;
            ClubId = clubId;
            MembershipTypeId = membershipTypeId;
            MembershipPlanId = membershipPlanId;
            Status = MembershipStatus.Active;
            ExtraDataJson = extraDataJson;
        }

        // Properties
        public Guid UserId { get; private set; }
        public Guid ClubId { get; private set; }
        public Club Club { get; private set; }
        public Guid MembershipTypeId { get; private set; }
        public MembershipType MembershipType { get; private set; }
        public Guid MembershipPlanId { get; private set; }
        public MembershipPlan MembershipPlan { get; private set; }
        public MembershipStatus Status { get; private set; }
        public Guid? MembershipApplicationId { get; private set; } = Guid.Empty;

        private readonly List<MembershipCycle> _MembershipCycles = new List<MembershipCycle>();
        public IReadOnlyList<MembershipCycle> MembershipCycles => _MembershipCycles.AsReadOnly();
        public string? ExtraDataJson { get; private set; }

        // Family members
        private readonly List<FamilyMember> _FamilyMembers = new();
        public IReadOnlyList<FamilyMember> FamilyMembers => _FamilyMembers.AsReadOnly();

        //public int FamilyMemberCount => _FamilyMembers.Count;

        // Cancellation tracking
        public DateTime? CancelledAt { get; private set; }
        public string? CancellationReason { get; private set; }

        #region Factory Methods

        #region Factory Methods

        public static Result<Membership> CreateFromPendingEnrollment(PendingEnrollment pendingEnrollment)
        {
            if (pendingEnrollment.Status != PendingEnrollmentStatus.WaitingForFirstPayment)
                return Error.Conflict(description: "Pending enrollment is not payable.");

            var membership = new Membership(
                Guid.NewGuid(),
                pendingEnrollment.UserId,
                pendingEnrollment.ClubId,
                pendingEnrollment.MembershipTypeId,
                pendingEnrollment.MembershipPlanId);

            membership.MembershipApplicationId = pendingEnrollment.MembershipApplicationId;

            var start = DateTime.UtcNow;
            var end = start.AddYears(pendingEnrollment.SubscriptionValidityInYears);

            var rules = InstallmentDto.ToInstallments(pendingEnrollment.GetInstallments().ToList());
            if (rules.IsError) return rules.TopError;


            var cycleResult = MembershipCycle.Create(
                membership.Id,
                membership.ClubId,
                membership.MembershipTypeId,
                membership.MembershipPlanId,
                start,
                end,
                pendingEnrollment.TotalPrice,
                pendingEnrollment.InstallmentTemplateId,
                rules.Value
                );

            if (cycleResult.IsError)
                return cycleResult.TopError;

            membership._MembershipCycles.Add(cycleResult.Value);
            return membership;
        }

        #endregion

        private static Result<Success> MapFamilyMembers(Membership membership, MembershipApplication app)
        {
            var familySection = app.TemplateSnapshot.Steps
                .SelectMany(s => s.Sections)
                .FirstOrDefault(sec => sec.Intent == SectionIntent.FamilyMembers);

            if (familySection == null) return Result.Success;

            var groupedMembers = app.Answers
                .Where(a => familySection.Fields.Any(f => f.Id == a.FieldDefinitionId))
                .GroupBy(a => a.InstanceIndex);

            foreach (var group in groupedMembers)
            {
                var fullName = group.FirstOrDefault(a => a.FieldKey == FamilyMemberField.FullName)?.Value;
                var relationshipStr = group.FirstOrDefault(a => a.FieldKey == FamilyMemberField.Relationship)?.Value;
                var dobStr = group.FirstOrDefault(a => a.FieldKey == FamilyMemberField.DateOfBirth)?.Value;

                var dob  = DateOnly.TryParse(dobStr, out var parsedDob) ? parsedDob : default;
                var rel  = Enum.TryParse<FamilyRelationship>(relationshipStr, true, out var parsedRel) ? parsedRel : default;

                var member = FamilyMember.Create(membership.Id,fullName, rel, dob);
                if(member.IsError) return member.TopError;

                 membership._FamilyMembers.Add(member.Value);
            }
            return Result.Success;
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
            if(!CurrentCycle.Period.IsExpired(DateTime.UtcNow))
                return MembershipErrors.CannotPeriodOfCurrentCycleNotEnded;

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

        public Result<Success> Renew(InstallmentTemplate? installmentTemplate)
        {
            if (Status != MembershipStatus.Active && Status != MembershipStatus.Expired)
                return Error.Validation("Membership.Cannot.Renew",
                    "Only active or expired memberships can be renewed.");


            var oldEndDate = CurrentCycle.Period.EndDate;
            var newStartDate = oldEndDate+TimeSpan.FromSeconds(1) > DateTime.UtcNow ? oldEndDate.AddSeconds(1) : DateTime.UtcNow;
            var newEndDate = newStartDate.AddYears(this.MembershipPlan.SubscriptionValidityInYears);

            if(installmentTemplate != null && !MembershipPlan.InstallmentTemplates.Any(i => i.InstallmentTemplateId == installmentTemplate.Id))
                return Error.Validation("Membership.InvalidInstallmentTemplate",
                    "The provided installment template is not valid for the current membership plan.");

             var installmentTemplateId = installmentTemplate?.Id;
            var installments = installmentTemplate != null ? installmentTemplate.Installments.ToList() : new List<Installment>
                {
                    Installment.Create(MembershipPlan.TotalPrice, 0, 1).Value
                };

                var cycleResult = MembershipCycle.Create(
                    Id,
                    ClubId,
                    MembershipTypeId,
                    MembershipPlanId,
                    newStartDate,
                    newEndDate,
                    MembershipPlan.TotalPrice,
                    installmentTemplateId,
                    installments);

            if (cycleResult.IsError)   return cycleResult.TopError;

            if (Status == MembershipStatus.Expired)
                Status = MembershipStatus.Active;

            RaiseDomainEvent(new MembershipRenewedDomainEvent(
                Id,
                UserId,
                oldEndDate,
                newEndDate,
                null,
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

        public MembershipCycle CurrentCycle =>
            _MembershipCycles.OrderByDescending(c => c.Period.StartDate)
                             .FirstOrDefault(c => !c.Period.IsExpired(DateTime.UtcNow));

        public bool IsActive()
        {
            return Status == MembershipStatus.Active &&
                   CurrentCycle.Period.IsActive(DateTime.UtcNow);
        }



        public int DaysRemaining()
        {
            if (Status == MembershipStatus.Expired || Status == MembershipStatus.Cancelled)
                return 0;

            return (CurrentCycle.Period.EndDate - DateTime.UtcNow).Days;
        }

        #endregion

    }
}
