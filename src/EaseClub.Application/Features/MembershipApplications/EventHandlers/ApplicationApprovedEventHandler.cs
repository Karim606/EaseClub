using EaseClub.Application.Common;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Features.Enrollments.Services;
using EaseClub.Application.Features.Notifications;
using EaseClub.Domain.Common.Interfaces;
using EaseClub.Domain.MembershipApplications;
using EaseClub.Domain.MembershipApplications.Repositories;
using EaseClub.Domain.MembershipPlans.Repositories;
using EaseClub.Domain.Notifications;
using Microsoft.Extensions.Logging;

namespace EaseClub.Application.Features.MembershipApplications.EventHandlers
{
    public class ApplicationApprovedEventHandler : DomainEventHandler<ApplicationApprovedEvent, ApplicationApprovedEventHandler>
    {
        private readonly EnrollmentManager _enrollmentManager;
        private readonly IMembershipPlanRepository _planRepository;
        private readonly IMembershipApplicationRepository _applicationRepository;

        public ApplicationApprovedEventHandler(
            EnrollmentManager enrollmentManager,
            IMembershipPlanRepository planRepository,
            IMembershipApplicationRepository applicationRepository,
            IUnitOfWork unitOfWork,
            ILogger<ApplicationApprovedEventHandler> logger,
            INotificationDispatcher notificationDispatcher,
            INotificationRepository notificationRepository) : base(notificationDispatcher, notificationRepository, unitOfWork, logger)
        {
            _enrollmentManager = enrollmentManager;
            _planRepository = planRepository;
            _applicationRepository = applicationRepository;
        }

        protected override async Task HandleEvent(ApplicationApprovedEvent evt, CancellationToken ct)
        {
            // 1. Fetch the application
            var app = await _applicationRepository.GetByIdAsync(evt.ApplicationId, ct);
            if (app == null)
            {
                _logger.LogError("Application {AppId} not found in ApprovedEventHandler", evt.ApplicationId);
                return;
            }

            // 2. Get the plan to ensure it exists
            var plan = await _planRepository.GetByIdAsync(app.MembershipPlanId, ct);
            if (plan == null)
            {
                _logger.LogError("Plan {PlanId} not found for approved application {AppId}", app.MembershipPlanId, app.Id);
                return;
            }

            // 3. Use EnrollmentManager to create the payable enrollment and invoice
            var result = await _enrollmentManager.CreatePayableEnrollmentAsync(
                app.MemberId,
                app.ClubId,
                plan,
                null, // Template is extracted inside manager from app snapshot if needed
                app,
                null,
                ct);

            if (result.IsError)
            {
                _logger.LogError("Failed to initialize enrollment for approved application {AppId}. ErrorCode: {ErrorCode}, Details: {Error}", 
                    app.Id, result.TopError.Code, result.TopError.Description);
                return;
            }

            // 4. Notify the user that they can now pay
            var metadata = System.Text.Json.JsonSerializer.Serialize(new
            {
                enrollmentId = result.Value.EnrollmentId,
                invoiceId = result.Value.InvoiceId
            });

            var notification = Notification.ForUser(
                app.MemberId,
                "Application Approved",
                $"Your application {app.TrackingNumber} has been approved. Please pay the first installment to activate your membership within 48 hours, before enrollment expiration.",
                NotificationType.MembershipApplicationApproved,
                metadata);

            await DispatchNotification(notification, ct);
            
            _logger.LogInformation("Successfully initialized enrollment {EnrollmentId} for approved application {AppId}", 
                result.Value.EnrollmentId, app.Id);
        }
    }
}
