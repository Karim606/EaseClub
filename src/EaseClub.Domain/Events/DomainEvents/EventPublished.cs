using System;

namespace EaseClub.Domain.Events.DomainEvents;

public record EventPublished(Guid PublishedEventId) : DomainEvent
{
    public EventPublished() : this(Guid.Empty) { }

    public new Guid AggregateId => PublishedEventId;
}
