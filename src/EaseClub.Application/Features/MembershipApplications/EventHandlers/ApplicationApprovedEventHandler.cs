using EaseClub.Application.Common;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Features.MembershipApplications.Queries;
using EaseClub.Application.Features.Notifications;
using EaseClub.Domain.MembershipApplications;
using EaseClub.Domain.MembershipApplications.Repositories;
using EaseClub.Domain.Memberships;
using EaseClub.Domain.Notifications;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipApplications.EventHandlers
{
    public class ApplicationApprovedEventHandler : DomainEventHandler<ApplicationApprovedEvent,ApplicationApprovedEventHandler>
    {
        //private readonly INotificationService _notificationService;
        private readonly IMembershipApplicationRepository _appRepo;
        private readonly IMembershipRepository _membershipRepo;
        public ApplicationApprovedEventHandler(
            //INotificationService notificationService,
            IMembershipApplicationRepository appRepo,
            IMembershipRepository membershipRepo,
            IUnitOfWork unitOfWork,
            ILogger<ApplicationApprovedEventHandler> logger,
            INotificationDispatcher notificationDispatcher,
            INotificationRepository notificationRepo):base(notificationDispatcher,notificationRepo,unitOfWork,logger)
        {
           // _notificationService = notificationService;
            _appRepo = appRepo;
            _membershipRepo = membershipRepo;
        }

        protected override async Task HandleEvent(ApplicationApprovedEvent evt, CancellationToken ct)
        {

            var app = await _appRepo.GetByIdAsync(evt.ApplicationId, ct);
            if (app != null)
            {
                _logger.LogWarning("Application {ApplicationId} not found", evt.ApplicationId);
                return;
            }

            var existing = await _membershipRepo.GetByApplicationIdAsync(evt.ApplicationId, ct);
            if (existing != null)
            {
                _logger.LogWarning(
                    "Membership already exists for application {ApplicationId}",
                    evt.ApplicationId);
                return;
            }

            var membership = Membership.CreateFromApplication(app);

            if (membership.IsError)
            {
                 _logger.LogError(
                "Failed to create membership from application {ApplicationId}: {Error}",
                evt.ApplicationId,
                membership.TopError.Description
                );

                return;
            }

            await  _membershipRepo.AddAsync(membership.Value);
            await  _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation(
            "Membership {MembershipId} created from application {ApplicationId}",
            membership.Value.Id,
            evt.ApplicationId);

            var notification = Notification.ForUser(evt.ApplicationOwnerId,"Membership Approved",$"Membership {membership.Value.Id} created from application {evt.ApplicationId}" +
                $"Pay your membership dued payments to activate your membership soon.", NotificationType.MembershipApplicationApproved);

            await DispatchNotification(notification);
        }
    }
}
