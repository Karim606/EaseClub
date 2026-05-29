namespace EaseClub.Domain.Events;

/// <summary>
/// Well-known field keys that the Pricing Engine can reference when policies are
/// assigned to an Event target.  These keys are populated into PricingContext.Data
/// during the registration pricing step.
/// </summary>
public static class EventFieldKeys
{
    /// <summary>Ticket category: Member | FamilyMember | Public | Guest</summary>
    public const string AttendeeCategory = "attendeeCategory";

    /// <summary>Whether the ticket requires active membership (true/false)</summary>
    public const string RequiresMembership = "requiresMembership";

    /// <summary>The base price of the selected ticket type (decimal string)</summary>
    public const string TicketBasePrice = "ticketBasePrice";

    /// <summary>Age of the individual attendee (integer string, nullable)</summary>
    public const string AttendeeAge = "attendeeAge";

    /// <summary>Gender of the individual attendee (Male/Female/Other, nullable)</summary>
    public const string AttendeeGender = "attendeeGender";

    /// <summary>Total number of attendees in this registration (integer string)</summary>
    public const string AttendeeCount = "attendeeCount";

    /// <summary>All supported field keys — passed to PricingPolicyAssignment.Create() for validation.</summary>
    public static readonly HashSet<string> All = new(StringComparer.OrdinalIgnoreCase)
    {
        AttendeeCategory,
        RequiresMembership,
        TicketBasePrice,
        AttendeeAge,
        AttendeeGender,
        AttendeeCount,
    };
}
