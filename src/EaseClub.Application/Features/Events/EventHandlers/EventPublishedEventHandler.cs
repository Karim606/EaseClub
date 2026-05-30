using EaseClub.Application.Common;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Features.Notifications;
using EaseClub.Domain.Events;
using EaseClub.Domain.Events.DomainEvents;
using EaseClub.Domain.Notifications;
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

        var notification = Notification.ForClub(
            @event.ClubId,
            "Event Published",
            $"Event '{@event.Name}' has been published successfully.",
            NotificationType.General);

        await DispatchNotification(notification,ct);

        _logger.LogInformation("Event '{@event.Name}' (Id: {@event.Id}) has been published successfully.", @event.Name, @event.Id);
    }
}
