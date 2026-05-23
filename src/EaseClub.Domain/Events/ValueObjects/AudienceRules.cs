using System;
using EaseClub.Domain.Events.Enums;

namespace EaseClub.Domain.Events.ValueObjects;

public record AccessRules
{
    public EventAccessType AccessType { get; init; }
    public bool RequiresMemberRegistrant { get; init; }

    private AccessRules(EventAccessType accessType, bool requiresMemberRegistrant)
    {
        AccessType = accessType;
        RequiresMemberRegistrant = requiresMemberRegistrant;
    }

    public static AccessRules For(EventAccessType accessType)
    {
        return accessType switch
        {
            EventAccessType.MembersOnly => new AccessRules(accessType, true),
            EventAccessType.MembersAndGuests => new AccessRules(accessType, true),
            EventAccessType.MembersAndFamily => new AccessRules(accessType, true),
            EventAccessType.Public => new AccessRules(accessType, false),
            _ => throw new ArgumentOutOfRangeException(nameof(accessType), "Invalid access type.")
        };
    }
}
