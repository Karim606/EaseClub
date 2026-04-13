using System;

namespace EaseClub.Domain.Events.DomainEvents;

public record EventPublished(Guid EventId) : DomainEvent
{
    public EventPublished() : this(Guid.Empty) { }
}
