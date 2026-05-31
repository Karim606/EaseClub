using EaseClub.Application.Common;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Features.Memberships;
using EaseClub.Application.Features.Notifications;
using EaseClub.Domain.Clubs;
using EaseClub.Domain.MembershipApplications;
using EaseClub.Domain.MembershipApplications.Repositories;
using EaseClub.Domain.MembershipPlans.Repositories;
using EaseClub.Domain.Memberships;
using EaseClub.Domain.Notifications;
using EaseClub.Domain.Payment;
using EaseClub.Domain.Payment.Enums;
using EaseClub.Domain.Payment.Events;
using EaseClub.Domain.Payment.Repositories;
using Microsoft.Extensions.Logging;

namespace EaseClub.Application.Features.Payment.EventHandlers
{
    /// <summary>
    /// Handles InvoicePaidEvent for brand new memberships (Direct Pay or Application Approval).
    /// </summary>
    public class NewEnrollmentInvoicePaidHandler : DomainEventHandler<InvoicePaidEvent, NewEnrollmentInvoicePaidHandler>
    {
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IMembershipRepository _membershipRepository;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IClubRepository _clubRepository;
        private readonly IMembershipApplicationRepository _applicationRepository;
        private readonly IMembershipPlanRepository _planRepository;
        private readonly MembershipNumberGenerator _membershipNumberGenerator;

        public NewEnrollmentInvoicePaidHandler(
            IEnrollmentRepository enrollmentRepository,
            IMembershipRepository membershipRepository,
            MembershipNumberGenerator membershipGenerator,
            IInvoiceRepository invoiceRepository,
            IClubRepository clubRepository,
            IMembershipApplicationRepository applicationRepository,
            IMembershipPlanRepository planRepository,
            IUnitOfWork unitOfWork,
            ILogger<NewEnrollmentInvoicePaidHandler> logger,
            INotificationDispatcher notificationDispatcher,
            INotificationRepository notificationRepository) : base(notificationDispatcher, notificationRepository, unitOfWork, logger)
        {
            _enrollmentRepository = enrollmentRepository;
            _membershipRepository = membershipRepository;
            _invoiceRepository = invoiceRepository;
            _clubRepository = clubRepository;
            _applicationRepository = applicationRepository;
            _planRepository = planRepository;
            _membershipNumberGenerator = membershipGenerator;
        }

        protected override async Task HandleEvent(InvoicePaidEvent evt, CancellationToken ct)
        {
            if (evt.type != BillingItemType.EnrollmentFirstInstallment)
                return;

            var enrollment = await _enrollmentRepository.GetByIdAsync(evt.billingItemId, ct);
            if (enrollment == null || enrollment.Source == EnrollmentSource.Renewal)
                return; // Not our responsibility

            //if (enrollment.IsExpired(DateTime.UtcNow))
            //{
            //    enrollment.MarkExpired();
            //    await _unitOfWork.SaveChangesAsync(ct);
            //    return;
            //}

            if (enrollment.Status == EnrollmentStatus.Completed)
                return;

            // 1. Mark Enrollment as Completed
            var completeResult = enrollment.MarkCompleted();
            if (completeResult.IsError)
            {
                _logger.LogError("Failed to mark new enrollment {EnrollmentId} as completed: {Error}", enrollment.Id, completeResult.TopError.Description);
                return;
            }

            // 2. Prevent Double Creation
            MembershipApplication? app = null;
            if (enrollment.MembershipApplicationId.HasValue)
            {
                var existing = await _membershipRepository.GetByApplicationIdAsync(enrollment.MembershipApplicationId.Value, ct);
                if (existing != null)
                {
                    _logger.LogWarning("Membership already exists for application {ApplicationId}, skipping creation.", enrollment.MembershipApplicationId.Value);
                    return;
                }

                app = await _applicationRepository.GetByIdAsync(enrollment.MembershipApplicationId.Value, ct);
            }

            // 3. Create Membership
            var club = await _clubRepository.GetByIdAsync(enrollment.ClubId, ct);
            var membershipNumber = await _membershipNumberGenerator.GenerateAsync(club.Code);

            // Fetch Plan for Capacity
            var plan = await _planRepository.GetByIdAsync(enrollment.MembershipPlanId, ct);
            if (plan == null)
            {
                _logger.LogError("Membership plan {PlanId} not found for enrollment {EnrollmentId}", enrollment.MembershipPlanId, enrollment.Id);
                return;
            }

            // Pass the application (if any) to map family members
            var membershipResult = Membership.CreateFromEnrollment(enrollment, membershipNumber, plan.MaxFamilyMembers, app);
            if (membershipResult.IsError)
            {
                _logger.LogError("Failed to create membership from enrollment {EnrollmentId}: {Error}", enrollment.Id, membershipResult.TopError.Description);
                return;
            }

            var membership = membershipResult.Value;

            // 4. Reconcile Payment
            await ReconcileFirstInstallmentAsync(membership, evt, ct);

            await _membershipRepository.AddAsync(membership, ct);

            // 5. Notify
            var notification = Notification.ForUser(
                enrollment.MemberId,
                "Membership Activated",
                "Your membership is now active after successful payment of the first installment.",
                NotificationType.MembershipActivated);

            await DispatchNotification(notification, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        private async Task ReconcileFirstInstallmentAsync(Membership membership, InvoicePaidEvent evt, CancellationToken ct)
        {
            var firstInstallment = membership.MembershipCycles.OrderByDescending(x => x.Period.StartDate).FirstOrDefault()?
                .Installments.OrderBy(x => x.Order).FirstOrDefault();

            if (firstInstallment == null) return;

            firstInstallment.MarkPaid(evt.Id);

            var invoice = await _invoiceRepository.GetByIdAsync(evt.Id);
            if (invoice != null)
            {
                invoice.Reconcile(firstInstallment.Id, BillingItemType.MembershipInstallment);
            }
        }
    }
}
