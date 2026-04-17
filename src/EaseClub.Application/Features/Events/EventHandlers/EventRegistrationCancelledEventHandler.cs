using EaseClub.Application.Common;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Events;
using EaseClub.Domain.Events.DomainEvents;
using EaseClub.Domain.Notifications;
using EaseClub.Application.Features.Notifications;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Events.EventHandlers;

public class EventRegistrationCancelledEventHandler : DomainEventHandler<EventRegistrationCancelled, EventRegistrationCancelledEventHandler>
{
    private readonly IEventRepository _eventRepo;

    public EventRegistrationCancelledEventHandler(
        IEventRepository eventRepo,
        IUnitOfWork unitOfWork,
        ILogger<EventRegistrationCancelledEventHandler> logger,
        INotificationDispatcher notificationDispatcher,
        INotificationRepository notificationRepo) : base(notificationDispatcher, notificationRepo, unitOfWork, logger)
    {
        _eventRepo = eventRepo;
    }

    protected override async Task HandleEvent(EventRegistrationCancelled evt, CancellationToken ct)
    {
        var @event = await _eventRepo.GetWithDetailsAsync(evt.EventId, ct);
        if (@event == null)
        {
            _logger.LogWarning("Event {EventId} not found", evt.EventId);
            return;
        }

        var registration = @event.Registrations.FirstOrDefault(r => r.Id == evt.RegistrationId);
        if (registration == null)
        {
            _logger.LogWarning("Registration {RegistrationId} not found for event {EventId}", evt.RegistrationId, evt.EventId);
            return;
        }

        var notification = Notification.ForUser(
            registration.RegistrantId,
            "Event Registration Cancelled",
            $"Your registration for the event '{@event.Name}' has been cancelled.",
            NotificationType.BookingCancelled);

        await DispatchNotification(notification);
    }
}
