using System;
using EaseClub.Domain.Common;

namespace EaseClub.Domain.Events.Entities;

public class Attendee : Entity
{
    public Guid EventRegistrationId { get; private set; }
    public Guid TicketTypeId { get; private set; }
    
    // An attendee can be a registered system user/member, or an unnamed guest/public user.
    public Guid? AttendeeId { get; private set; }
    public string AttendeeName { get; private set; }

    internal Attendee(Guid eventRegistrationId, Guid ticketTypeId, Guid? attendeeId, string attendeeName) : base()
    {
        EventRegistrationId = eventRegistrationId;
        TicketTypeId = ticketTypeId;
        AttendeeId = attendeeId;
        AttendeeName = attendeeName;
    }

    private Attendee() { }
}
