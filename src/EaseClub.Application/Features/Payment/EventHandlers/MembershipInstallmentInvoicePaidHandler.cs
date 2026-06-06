using EaseClub.Application.Common;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Features.Notifications;
using EaseClub.Domain.Common;
using EaseClub.Domain.Memberships;
using EaseClub.Domain.MembershipPlans;
using EaseClub.Domain.Notifications;
using EaseClub.Domain.Payment.Enums;
using EaseClub.Domain.Payment.Events;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Payment.EventHandlers
{
    /// <summary>
    /// Handles InvoicePaidEvent for MembershipInstallment items to mark installments as paid.
    /// </summary>
    public class MembershipInstallmentInvoicePaidHandler : DomainEventHandler<InvoicePaidEvent, MembershipInstallmentInvoicePaidHandler>
    {
        private readonly IMembershipRepository _membershipRepository;

        public MembershipInstallmentInvoicePaidHandler(
            IMembershipRepository membershipRepository,
            IUnitOfWork unitOfWork,
            ILogger<MembershipInstallmentInvoicePaidHandler> logger,
            INotificationDispatcher notificationDispatcher,
            INotificationRepository notificationRepository) 
            : base(notificationDispatcher, notificationRepository, unitOfWork, logger)
        {
            _membershipRepository = membershipRepository;
        }

        protected override async Task HandleEvent(InvoicePaidEvent evt, CancellationToken ct)
        {
            if (evt.type != BillingItemType.MembershipInstallment)
                return;

            _logger.LogInformation("Processing InvoicePaidEvent for MembershipInstallment {Id}.", evt.billingItemId);

            var installment = await _membershipRepository.GetInstallmentByIdAsync(evt.billingItemId, ct);
            if (installment == null)
            {
                _logger.LogError("Membership Installment {Id} not found during payment reconciliation.", evt.billingItemId);
                return;
            }

            if (installment.Status == InstallmentStatus.Paid)
            {
                _logger.LogWarning("Membership Installment {Id} is already marked as paid.", installment.Id);
                return;
            }

            // Mark the installment as paid in the domain model
            var payResult = installment.MarkPaid(evt.Id);
            if (payResult.IsError)
            {
                _logger.LogError("Failed to mark installment {Id} as paid: {Error}", installment.Id, payResult.TopError.Description);
                return;
            }

            // Create notification for the user
            var notification = Notification.ForUser(
                evt.UserId,
                "Installment Paid",
                $"Your payment of {evt.Amount} for installment '{installment.ReadableId}' has been received and confirmed.",
                NotificationType.PaymentSuccessful);

            await DispatchNotification(notification, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("Successfully marked installment {Id} as paid and notified user.", installment.Id);
        }
    }
}
