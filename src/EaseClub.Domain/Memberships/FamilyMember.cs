using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;

namespace EaseClub.Domain.Memberships;

public class FamilyMember : AuditableEntity
{
    private FamilyMember() { }

    private FamilyMember(
        Guid id,
        Guid membershipId,
        string fullName,
        FamilyRelationship relationship,
        DateOnly dateOfBirth) : base(id)
    {
        MembershipId = membershipId;
        FullName = fullName;
        Relationship = relationship;
        DateOfBirth = dateOfBirth;
    }

    // --- First-Class Properties ---
    public Guid MembershipId { get; private set; }
    public Membership Membership { get; private set; }
    public string FullName { get; private set; } = null!;
    public FamilyRelationship Relationship { get; private set; }
    public DateOnly DateOfBirth { get; private set; }

    // --- Dynamic Data for Custom Template Fields ---
    private readonly Dictionary<string, string> _additionalData = new();
    public IReadOnlyDictionary<string, string> AdditionalData => _additionalData;

    internal static Result<FamilyMember> Create(
        Guid membershipId,
        string fullName,
        FamilyRelationship relationship,
        DateOnly dateOfBirth)
    {
        if (string.IsNullOrWhiteSpace(fullName)) return Error.Validation("FamilyMember.InvalidName", "First name is required.");
        if (membershipId == Guid.Empty) return Error.Validation("FamilyMember.InvalidMembershipId", "Membership ID is required.");


        return new FamilyMember(Guid.NewGuid(), membershipId, fullName, relationship,dateOfBirth);
    }

    public void UpdateInformation(string fullName, FamilyRelationship relationship)
    {
        FullName = fullName;
        Relationship = relationship;
    }

    public void SetAdditionalData(string key, string value)
    {
        _additionalData[key.ToLower().Trim()] = value;
    }

    public int GetAge(DateOnly? asOf = null)
    {
        // If no date is provided, use today's date
        var today = asOf ?? DateOnly.FromDateTime(DateTime.Now);

        var age = today.Year - DateOfBirth.Year;

        // Check if the birthday has occurred yet this year
        if (DateOfBirth > today.AddYears(-age))
            age--;

        return age;
    }

    public bool IsMinor(DateOnly? asOf = null)
    {
        return GetAge(asOf) < 18;
    }
}
