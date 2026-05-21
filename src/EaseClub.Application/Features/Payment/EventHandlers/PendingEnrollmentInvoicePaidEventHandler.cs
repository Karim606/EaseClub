using EaseClub.Application.Common;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Features.Memberships;
using EaseClub.Application.Features.Notifications;
using EaseClub.Domain.Clubs;
using EaseClub.Domain.Memberships;
using EaseClub.Domain.Notifications;
using EaseClub.Domain.Payment;
using EaseClub.Domain.Payment.Enums;
using EaseClub.Domain.Payment.Events;
using EaseClub.Domain.Payment.Repositories;
using Microsoft.Extensions.Logging;

namespace EaseClub.Application.Features.Payment.EventHandlers
{
    public class PendingEnrollmentInvoicePaidEventHandler : DomainEventHandler<InvoicePaidEvent, PendingEnrollmentInvoicePaidEventHandler>
    {
        private readonly IPendingEnrollmentRepository _pendingEnrollmentRepository;
        private readonly IMembershipRepository _membershipRepository;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IClubRepository _clubRepository;
        private readonly MembershipNumberGenerator _membershipNumberGenerator;

        public PendingEnrollmentInvoicePaidEventHandler(
            IPendingEnrollmentRepository pendingEnrollmentRepository,
            IMembershipRepository membershipRepository,
            MembershipNumberGenerator membershipGenerator,
            IInvoiceRepository invoiceRepository,
            IClubRepository clubRepository,
            IUnitOfWork unitOfWork,
            ILogger<PendingEnrollmentInvoicePaidEventHandler> logger,
            INotificationDispatcher notificationDispatcher,
            INotificationRepository notificationRepository) : base(notificationDispatcher, notificationRepository, unitOfWork, logger)
        {
            _pendingEnrollmentRepository = pendingEnrollmentRepository;
            _membershipRepository = membershipRepository;
            _invoiceRepository = invoiceRepository;
            _clubRepository = clubRepository;
            _membershipNumberGenerator = membershipGenerator;
        }

        protected override async Task HandleEvent(InvoicePaidEvent evt, CancellationToken ct)
        {
            if (evt.type != BillingItemType.PendingEnrollmentFirstInstallment)
                return;

            var pendingEnrollment = await _pendingEnrollmentRepository.GetByIdAsync(evt.billingItemId, ct);
            if (pendingEnrollment == null)
            {
                _logger.LogWarning("Pending enrollment {PendingEnrollmentId} not found", evt.billingItemId);
                return;
            }
            if (pendingEnrollment.IsExpired(DateTime.UtcNow))
            {
                pendingEnrollment.MarkExpired();
                await _unitOfWork.SaveChangesAsync(ct);
                return;
            }
            if (pendingEnrollment.MembershipApplicationId.HasValue)
            {
                var existing = await _membershipRepository.GetByApplicationIdAsync(pendingEnrollment.MembershipApplicationId.Value, ct);
                if (existing != null)
                    return;
            }
            var club = await _clubRepository.GetByIdAsync(pendingEnrollment.ClubId, ct);
            var membershipNumber = await _membershipNumberGenerator.GenerateAsync(club.Code);

            var membershipResult = Membership.CreateFromPendingEnrollment(pendingEnrollment,membershipNumber);
            if (membershipResult.IsError)
            {
                _logger.LogError(
                    "Failed to create membership from pending enrollment {PendingEnrollmentId}: {Error}",
                    pendingEnrollment.Id,
                    membershipResult.TopError.Description);
                return;
            }
            var membership = membershipResult.Value;
            var firstInstallment = membership.GetCurrentCycle()?.Installments.OrderBy(x => x.Order).FirstOrDefault();
            if (firstInstallment == null)
            {
                _logger.LogError("No first installment generated for membership {MembershipId}", membership.Id);
                return;
            }
            var markPaidResult = firstInstallment.MarkPaid(evt.Id);
            if (markPaidResult.IsError)
            {
                _logger.LogError(
                    "Failed to mark first installment as paid for membership {MembershipId}: {Error}",
                    membership.Id,
                    markPaidResult.TopError.Description);
                return;
            }
            var invoice = await _invoiceRepository.GetByIdAsync(evt.Id);
            if (invoice == null)
            {

                _logger.LogError("Invoice doesnt exists");
                return;
            }
            invoice.Reconcile(firstInstallment.Id, BillingItemType.MembershipInstallment);

            var completeResult = pendingEnrollment.MarkCompleted();
            if (completeResult.IsError)
            {
                _logger.LogError(
                    "Failed to complete pending enrollment {PendingEnrollmentId}: {Error}",
                    pendingEnrollment.Id,
                    completeResult.TopError.Description);
                return;
            }
            await _membershipRepository.AddAsync(membership, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var notification = Notification.ForUser(
                pendingEnrollment.UserId,
                "Membership Activated",
                "Your membership is now active after successful payment of the first installment.",
                NotificationType.MembershipApplicationApproved);

            await DispatchNotification(notification,ct);
        }
    }
}
