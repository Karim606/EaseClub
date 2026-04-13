using System;
using System.Collections.Generic;
using EaseClub.Domain.Events.Enums;

namespace EaseClub.Domain.Events.ValueObjects;

public record AudienceRules
{
    public Audience Audience { get; init; }
    public IReadOnlyCollection<AttendeeCategory> AllowedCategories { get; init; }
    public bool RequiresMemberRegistrant { get; init; }

    private AudienceRules(Audience audience, IReadOnlyCollection<AttendeeCategory> allowedCategories, bool requiresMemberRegistrant)
    {
        Audience = audience;
        AllowedCategories = allowedCategories;
        RequiresMemberRegistrant = requiresMemberRegistrant;
    }

    public static AudienceRules For(Audience audience)
    {
        return audience switch
        {
            Audience.MembersOnly => new AudienceRules(audience, new[] { AttendeeCategory.Member }, true),
            Audience.MembersAndGuests => new AudienceRules(audience, new[] { AttendeeCategory.Member, AttendeeCategory.Guest }, true),
            Audience.MembersWithFamily => new AudienceRules(audience, new[] { AttendeeCategory.Member, AttendeeCategory.FamilyMember }, true),
            Audience.MembersWithFamilyAndGuests => new AudienceRules(audience, new[] { AttendeeCategory.Member, AttendeeCategory.FamilyMember, AttendeeCategory.Guest }, true),
            Audience.Public => new AudienceRules(audience, new[] { AttendeeCategory.Member, AttendeeCategory.FamilyMember, AttendeeCategory.Guest, AttendeeCategory.Public }, false),
            _ => throw new ArgumentOutOfRangeException(nameof(audience), "Invalid audience type.")
        };
    }
}
