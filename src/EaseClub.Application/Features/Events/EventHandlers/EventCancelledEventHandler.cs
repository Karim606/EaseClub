using EaseClub.Application.Common;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Features.Notifications;
using EaseClub.Domain.Events;
using EaseClub.Domain.Events.DomainEvents;
using EaseClub.Domain.Notifications;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Events.EventHandlers;

public class EventCancelledEventHandler : DomainEventHandler<EventCancelled, EventCancelledEventHandler>
{
    private readonly IEventRepository _eventRepo;

    public EventCancelledEventHandler(
        IEventRepository eventRepo,
        IUnitOfWork unitOfWork,
        ILogger<EventCancelledEventHandler> logger,
        INotificationDispatcher notificationDispatcher,
        INotificationRepository notificationRepo) : base(notificationDispatcher, notificationRepo, unitOfWork, logger)
    {
        _eventRepo = eventRepo;
    }

    protected override async Task HandleEvent(EventCancelled evt, CancellationToken ct)
    {
        var @event = await _eventRepo.GetWithDetailsAsync(evt.EventId, ct);
        if (@event == null)
        {
            _logger.LogWarning("Event {EventId} not found", evt.EventId);
            return;
        }

        var registrants = @event.Registrations.Select(r => r.RegistrantId).Distinct().ToList();

        foreach (var registrantId in registrants)
        {
            var notification = Notification.ForUser(
                registrantId,
                "Event Cancelled",
                $"We are sorry to inform you that the event '{@event.Name}' has been cancelled.",
                NotificationType.EventCancelled);

            await DispatchNotification(notification,ct);
        }
    }
}
