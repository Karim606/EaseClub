using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;

namespace EaseClub.Domain.Memberships.ValueObjects
{
    public record FamilyMemberInfo
    {
        public Guid FamilyMemberId { get; init; }
        public string FullName { get; init; }
        public string Relationship { get; init; } // Son, Daughter, Spouse, etc.
        public DateTime DateOfBirth { get; init; }
        public DateTime AddedAt { get; init; }

        private FamilyMemberInfo(Guid id, string fullName, string relationship, DateTime dateOfBirth, DateTime addedAt)
        {
            FamilyMemberId = id;
            FullName = fullName;
            Relationship = relationship;
            DateOfBirth = dateOfBirth;
            AddedAt = addedAt;
        }

        public static Result<FamilyMemberInfo> Create(
            Guid id,
            string fullName,
            string relationship,
            DateTime dateOfBirth)
        {
            if (id == Guid.Empty)
                return Error.Validation("FamilyMember.Id.Required", "Family member ID is required.");

            if (string.IsNullOrWhiteSpace(fullName))
                return Error.Validation("FamilyMember.Name.Required", "Family member name is required.");

            if (string.IsNullOrWhiteSpace(relationship))
                return Error.Validation("FamilyMember.Relationship.Required", "Relationship is required.");

            if (dateOfBirth > DateTime.UtcNow)
                return Error.Validation("FamilyMember.DateOfBirth.Invalid", "Date of birth cannot be in the future.");

            return new FamilyMemberInfo(id, fullName, relationship, dateOfBirth, DateTime.UtcNow);
        }

        public int GetAge(DateTime? asOf = null)
        {
            var referenceDate = asOf ?? DateTime.UtcNow;
            var age = referenceDate.Year - DateOfBirth.Year;
            if (DateOfBirth.Date > referenceDate.AddYears(-age))
                age--;
            return age;
        }

        public bool IsMinor(DateTime? asOf = null)
        {
            return GetAge(asOf) < 18;
        }
    }
}