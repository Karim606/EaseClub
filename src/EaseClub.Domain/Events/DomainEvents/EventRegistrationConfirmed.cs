using System;

namespace EaseClub.Domain.Events.DomainEvents;

public record EventRegistrationConfirmed(Guid RegistrationId, Guid EventId) : DomainEvent
{
    public EventRegistrationConfirmed() : this(Guid.Empty, Guid.Empty) { }
}
