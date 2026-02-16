using EaseClub.Domain.Common;

namespace EaseClub.Domain.Memberships.Errors
{
    public static class MembershipErrors
    {
        public static Error UserIdRequired => Error.Validation(
            code: "Membership.UserId.Required",
            description: "User ID is required to create a membership.");

        public static Error ClubIdRequired => Error.Validation(
            code: "Membership.ClubId.Required",
            description: "Club ID is required to create a membership.");

        public static Error MembershipTypeIdRequired => Error.Validation(
            code: "Membership.MembershipTypeId.Required",
            description: "Membership type ID is required.");

        public static Error MembershipPlanIdRequired => Error.Validation(
            code: "Membership.MembershipPlanId.Required",
            description: "Membership plan ID is required.");

        public static Error InvalidDateRange => Error.Validation(
            code: "Membership.DateRange.Invalid",
            description: "Start date must be before end date.");

        public static Error AlreadyActive => Error.Conflict(
            code: "Membership.Already.Active",
            description: "Membership is already active.");

        public static Error NotActive => Error.Validation(
            code: "Membership.Not.Active",
            description: "Membership must be active to perform this operation.");

        public static Error AlreadyCancelled => Error.Conflict(
            code: "Membership.Already.Cancelled",
            description: "Membership is already cancelled.");

        public static Error AlreadyExpired => Error.Conflict(
            code: "Membership.Already.Expired",
            description: "Membership has already expired.");

        public static Error CannotActivateExpired => Error.Validation(
            code: "Membership.Cannot.Activate.Expired",
            description: "Cannot activate an expired membership.");

        public static Error CannotActivateCancelled => Error.Validation(
            code: "Membership.Cannot.Activate.Cancelled",
            description: "Cannot activate a cancelled membership.");

        public static Error InvalidRenewalDate => Error.Validation(
            code: "Membership.Renewal.Date.Invalid",
            description: "Renewal date must be after current end date.");

        public static Error MaxFamilyMembersReached => Error.Validation(
            code: "Membership.FamilyMembers.MaxReached",
            description: "Maximum number of family members has been reached.");

        public static Error FamilyMemberNotFound => Error.NotFound(
            code: "Membership.FamilyMember.NotFound",
            description: "Family member not found in this membership.");

        public static Error InvalidUpgrade => Error.Validation(
            code: "Membership.Upgrade.Invalid",
            description: "Cannot upgrade to the same or lower membership type.");

        public static Error InvalidDowngrade => Error.Validation(
            code: "Membership.Downgrade.Invalid",
            description: "Cannot downgrade to the same or higher membership type.");

        public static Error UpgradeNotAllowed => Error.Validation(
            code: "Membership.Upgrade.NotAllowed",
            description: "Upgrade is not allowed for this membership type.");

        public static Error DowngradeNotAllowed => Error.Validation(
            code: "Membership.Downgrade.NotAllowed",
            description: "Downgrade is not allowed for this membership type.");
    }
}