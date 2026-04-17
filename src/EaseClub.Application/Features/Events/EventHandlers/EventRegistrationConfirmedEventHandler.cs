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

public class EventRegistrationConfirmedEventHandler : DomainEventHandler<EventRegistrationConfirmed, EventRegistrationConfirmedEventHandler>
{
    private readonly IEventRepository _eventRepo;

    public EventRegistrationConfirmedEventHandler(
        IEventRepository eventRepo,
        IUnitOfWork unitOfWork,
        ILogger<EventRegistrationConfirmedEventHandler> logger,
        INotificationDispatcher notificationDispatcher,
        INotificationRepository notificationRepo) : base(notificationDispatcher, notificationRepo, unitOfWork, logger)
    {
        _eventRepo = eventRepo;
    }

    protected override async Task HandleEvent(EventRegistrationConfirmed evt, CancellationToken ct)
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
            "Event Registration Confirmed",
            $"Your registration for the event '{@event.Name}' has been successfully confirmed. We look forward to seeing you there!",
            NotificationType.BookingConfirmed);

        await DispatchNotification(notification);
    }
}
