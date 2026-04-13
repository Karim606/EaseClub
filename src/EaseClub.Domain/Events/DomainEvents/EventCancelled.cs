using System;

namespace EaseClub.Domain.Events.DomainEvents;

public record EventCancelled(Guid EventId) : DomainEvent
{
    public EventCancelled() : this(Guid.Empty) { }
}
