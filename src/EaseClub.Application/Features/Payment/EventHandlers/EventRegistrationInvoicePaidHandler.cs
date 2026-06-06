using EaseClub.Application.Common;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Features.Notifications;
using EaseClub.Domain.Common;
using EaseClub.Domain.Events;
using EaseClub.Domain.Notifications;
using EaseClub.Domain.Payment.Enums;
using EaseClub.Domain.Payment.Events;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Payment.EventHandlers
{
    /// <summary>
    /// Handles InvoicePaidEvent for EventRegistration items to confirm registrations automatically.
    /// </summary>
    public class EventRegistrationInvoicePaidHandler : DomainEventHandler<InvoicePaidEvent, EventRegistrationInvoicePaidHandler>
    {
        private readonly IEventRepository _eventRepository;

        public EventRegistrationInvoicePaidHandler(
            IEventRepository eventRepository,
            IUnitOfWork unitOfWork,
            ILogger<EventRegistrationInvoicePaidHandler> logger,
            INotificationDispatcher notificationDispatcher,
            INotificationRepository notificationRepository) 
            : base(notificationDispatcher, notificationRepository, unitOfWork, logger)
        {
            _eventRepository = eventRepository;
        }

        protected override async Task HandleEvent(InvoicePaidEvent evt, CancellationToken ct)
        {
            if (evt.type != BillingItemType.EventRegistration)
                return;

            _logger.LogInformation("Processing InvoicePaidEvent for EventRegistration {Id}.", evt.billingItemId);

            var registration = await _eventRepository.GetRegistrationByIdAsync(evt.billingItemId, ct);
            if (registration == null)
            {
                _logger.LogError("Event Registration {Id} not found during payment reconciliation.", evt.billingItemId);
                return;
            }

            var @event = await _eventRepository.GetWithDetailsAsync(registration.EventId, ct);
            if (@event == null)
            {
                _logger.LogError("Event {Id} not found for registration {RegId}.", registration.EventId, registration.Id);
                return;
            }

            // Confirm the registration in the event domain model
            var confirmResult = @event.ConfirmRegistration(registration.Id);
            if (confirmResult.IsError)
            {
                _logger.LogError("Failed to confirm registration {Id}: {Error}", registration.Id, confirmResult.TopError.Description);
                return;
            }

            // Create notification for the user
            var notification = Notification.ForUser(
                registration.RegistrantId,
                "Event Booking Confirmed!",
                $"Your registration for '{@event.Name}' has been successfully paid and confirmed.",
                NotificationType.BookingConfirmed);

            await DispatchNotification(notification, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("Successfully confirmed registration {Id} and notified user.", registration.Id);
        }
    }
}
