using EaseClub.Application.Common;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Events;
using EaseClub.Domain.Events.DomainEvents;
using EaseClub.Domain.Notifications;
using EaseClub.Application.Features.Notifications;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Events.EventHandlers;

public class EventPublishedEventHandler : DomainEventHandler<EventPublished, EventPublishedEventHandler>
{
    private readonly IEventRepository _eventRepo;

    public EventPublishedEventHandler(
        IEventRepository eventRepo,
        IUnitOfWork unitOfWork,
        ILogger<EventPublishedEventHandler> logger,
        INotificationDispatcher notificationDispatcher,
        INotificationRepository notificationRepo) : base(notificationDispatcher, notificationRepo, unitOfWork, logger)
    {
        _eventRepo = eventRepo;
    }

    protected override async Task HandleEvent(EventPublished evt, CancellationToken ct)
    {
        var @event = await _eventRepo.GetWithDetailsAsync(evt.EventId, ct);
        if (@event == null)
        {
            _logger.LogWarning("Event {EventId} not found", evt.EventId);
            return;
        }

        _logger.LogInformation("Event {@eventId} '{@eventName}' was published successfully.", @event.Id, @event.Name);

        // Here we could notify all club members or users that match the event's audience criteria
        // For now, logging stringly that the event has been published.
    }
}
