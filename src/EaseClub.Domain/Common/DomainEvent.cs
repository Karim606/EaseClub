using MediatR;

/// <summary>
/// Base class for all domain events.
/// Domain events represent something that happened in the domain that domain experts care about.
/// They are used for communication between aggregates and triggering side effects.
/// </summary>
public abstract record DomainEvent(DateTime OccurredOn) : INotification
{
    /// <summary>
    /// Unique identifier for this domain event instance
    /// </summary>
    public Guid EventId { get; init; } = Guid.NewGuid();

    /// <summary>
    /// When this event occurred (UTC)
    /// </summary>
    public DateTime OccurredOn { get; init; } = OccurredOn;
}