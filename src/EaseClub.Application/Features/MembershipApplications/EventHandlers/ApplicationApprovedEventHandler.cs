using EaseClub.Application.Common;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Features.Memberships;
using EaseClub.Application.Features.Notifications;
using EaseClub.Application.Features.Payment;
using EaseClub.Domain.Common;
using EaseClub.Domain.MembershipApplications;
using EaseClub.Domain.MembershipApplications.Repositories;
using EaseClub.Domain.MembershipPlans;
using EaseClub.Domain.MembershipPlans.Repositories;
using EaseClub.Domain.Memberships;
using EaseClub.Domain.Notifications;
using EaseClub.Domain.Payment;
using EaseClub.Domain.Payment.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EaseClub.Application.Features.MembershipApplications.EventHandlers
{
    public class ApplicationApprovedEventHandler : DomainEventHandler<ApplicationApprovedEvent,ApplicationApprovedEventHandler>
    {
        private readonly IMembershipApplicationRepository _appRepo;
        private readonly IPendingEnrollmentRepository _pendingEnrollmentRepo;
        private readonly IInvoiceRepository _invoiceRepo;
        private readonly IMembershipPlanRepository _planRepo;
        public ApplicationApprovedEventHandler(
            IMembershipApplicationRepository appRepo,
            IInvoiceRepository invoiceRepo,
            IMembershipPlanRepository planRepo,
            IPendingEnrollmentRepository pendingEnrollmentRepo,
            IUnitOfWork unitOfWork,
            ILogger<ApplicationApprovedEventHandler> logger,
            INotificationDispatcher notificationDispatcher,
            INotificationRepository notificationRepo):base(notificationDispatcher,notificationRepo,unitOfWork,logger)
        {
            _appRepo = appRepo;
            _pendingEnrollmentRepo = pendingEnrollmentRepo;
            _invoiceRepo = invoiceRepo;
            _planRepo = planRepo;
        }

        protected override async Task HandleEvent(ApplicationApprovedEvent evt, CancellationToken ct)
        {

            var app = await _appRepo.GetByIdAsync(evt.ApplicationId, ct);
            if (app == null)
            {
                _logger.LogError("Application {ApplicationId} not found", evt.ApplicationId);
                return;
            }
            var existingPending = await _pendingEnrollmentRepo.GetActiveByApplicationIdAsync(app.Id, ct);
            if (existingPending != null) {
                _logger.LogError("Active pending enrollment already exists for application {ApplicationId}", app.Id);
                return;
                    }
            var plan = await _planRepo.GetByIdAsync(app.MembershipPlanId,ct);
            if (plan == null)
            {
                _logger.LogError("plan with id:{planId} not found", app.MembershipPlanId);
                return;
            }
            var pendingEnrollmentResult = PendingEnrollment.CreateFromApprovedApplication(app,plan);
            if (pendingEnrollmentResult.IsError) {
                _logger.LogError("Failed to create pending enrollment for application {ApplicationId}: {Errors}", app.Id, pendingEnrollmentResult.Errors);
                return;
                    }
            var pendingEnrollment = pendingEnrollmentResult.Value;

            var invoiceResult = Invoice.Create(
                pendingEnrollment,
                pendingEnrollment.ClubId,
                pendingEnrollment.UserId,
                pendingEnrollment.Amount);

            if (invoiceResult.IsError)
            {
                _logger.LogError("Failed to create invoice for pending enrollment {PendingEnrollmentId}: {Errors}", pendingEnrollment.Id, invoiceResult.Errors);
                return;
            }
            var invoice = invoiceResult.Value;
            var attachResult = pendingEnrollment.AttachFirstInvoice(invoice.Id);
            if (attachResult.IsError)
            {
                _logger.LogError("Failed to attach invoice {InvoiceId} to pending enrollment {PendingEnrollmentId}: {Errors}", invoice.Id, pendingEnrollment.Id, attachResult.Errors);
                return;
            }
            await _invoiceRepo.AddAsync(invoice, ct);

            await _unitOfWork.SaveChangesAsync(ct);


            var notification = Notification.ForUser(
                evt.ApplicationOwnerId,
                "Membership Approved",
                $"Application {app.TrackingNumber} was approved. Complete the first installment payment to create and activate your membership.",
                NotificationType.MembershipApplicationApproved);

            await DispatchNotification(notification, ct);
        }
    }
}
