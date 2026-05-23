using System;

namespace EaseClub.Domain.Events.DomainEvents;

public record EventRegistrationCreated(Guid RegistrationId, Guid EventId) : DomainEvent
{
    public EventRegistrationCreated() : this(Guid.Empty, Guid.Empty) { }
}
