using System;
using System.Collections.Generic;
using System.Linq;
using EaseClub.Domain.Events.Enums;

namespace EaseClub.Domain.Events.ValueObjects;

public record AccessRules
{
    public EventAccessType AccessType { get; init; }
    public bool RequiresMemberRegistrant { get; init; }
    public IReadOnlyCollection<AttendeeCategory> AllowedCategories { get; init; }

    private AccessRules(EventAccessType accessType, bool requiresMemberRegistrant, IReadOnlyCollection<AttendeeCategory> allowedCategories)
    {
        AccessType = accessType;
        RequiresMemberRegistrant = requiresMemberRegistrant;
        AllowedCategories = allowedCategories;
    }

    public bool IsCategoryAllowed(AttendeeCategory category) => AllowedCategories.Contains(category);

    public static AccessRules For(EventAccessType accessType)
    {
        return accessType switch
        {
            EventAccessType.MembersOnly => new AccessRules(
                accessType, 
                true, 
                new[] { AttendeeCategory.Member, AttendeeCategory.FamilyMember, AttendeeCategory.Guest }),
            EventAccessType.Public => new AccessRules(
                accessType, 
                false, 
                new[] { AttendeeCategory.Member, AttendeeCategory.FamilyMember, AttendeeCategory.Public, AttendeeCategory.Guest }),
            _ => throw new ArgumentOutOfRangeException(nameof(accessType), "Invalid access type.")
        };
    }
}
