using System;

namespace EaseClub.Domain.Events.DomainEvents;

public record EventRegistrationCancelled(Guid RegistrationId, Guid EventId) : DomainEvent
{
    public EventRegistrationCancelled() : this(Guid.Empty, Guid.Empty) { }
}
