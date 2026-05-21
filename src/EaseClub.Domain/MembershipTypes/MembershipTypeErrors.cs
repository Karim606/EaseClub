using EaseClub.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.MembershipTypes
{
    public static class MembershipTypeErrors
    {
        public static Error MembershipTypeAlreadyAllBranchesPermitted = Error.Conflict(code: "MembershipType.AlreadyAllBranchesPermitted",
            description: "Membership type already allows all branches.");

        public static Error MembershipTypeNameMustBeUniquePerClub = Error.Conflict(code: "MembershipType.NameMustBeUniquePerClub",
            description: "Membership type name must be unique within the club.");

        public static Error MembershipTypeFamilyAlreadyEnabled = Error.Conflict(code: "MembershipType.FamilyAlreadyEnabled",
            description: "Membership type family membership is already enabled.");

        public static Error MembershipTypeFamilyAlreadyDisabled = Error.Conflict(code: "MembershipType.FamilyAlreadyDisabled",
            description: "Membership type family membership is already disabled.");

        public static Error RestrictedToBranchListMustBeGreaterThanZero = Error.Validation
            (code: "MembershipType.RestrictedToBranchListMustBeGreaterThanZero",
            description: "The list of branches to restrict access to must contain at least one branch.");

        public static Error MaxFamilyMembersMustBeGreaterThanZero = Error.Validation
            (code: "MembershipType.MaxFamilyMembersMustBeGreaterThanZero",
            description: "Max family members must be greater than zero.");

        public static Error MembershipTypeNameMustNotBeEmpty = Error.Validation
            (code: "MembershipType.NameMustNotBeEmpty",
            description: "Membership type name must not be empty.");

        public static Error ClubIdIsRequired = Error.Validation
            (code: "MembershipType.ClubIdIsRequired",
            description: "Club ID is required to create a membership type.");

        public static Error BranchDoesNotExist(IEnumerable<Guid> branchIds) => Error.NotFound(
            code: "MembershipType.BranchDoesNotExist",
            description: $"One or more branches do not exist: {string.Join(", ", branchIds)}");

    }
}
