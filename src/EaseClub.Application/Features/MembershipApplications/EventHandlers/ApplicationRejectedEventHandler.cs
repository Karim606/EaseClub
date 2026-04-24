using EaseClub.Application.Common;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Features.Notifications;
using EaseClub.Domain.MembershipApplications;
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
    public class ApplicationRejectedEventHandler
         : DomainEventHandler<ApplicationRejectedEvent, ApplicationRejectedEventHandler>
    {

        public ApplicationRejectedEventHandler(
            INotificationDispatcher dispatcher,
            INotificationRepository notificationRepo,
            IUnitOfWork unitOfWork,
            ILogger<ApplicationRejectedEventHandler> logger
        ) : base(dispatcher,notificationRepo,unitOfWork,logger)
        {

        }

        protected override async Task HandleEvent(ApplicationRejectedEvent evt, CancellationToken ct)
        {
            // If rejection is not visible to the user we do nothing
            //if (!evt.VisibleToUser)
            //{
            //    _logger.LogInformation(
            //        "Application {ApplicationId} rejected but not visible to user",
            //        evt.ApplicationId);
            //    return;
            //}
            _logger.LogInformation(
                "Application {ApplicationId} rejected. Reason: {Reason}",
                evt.ApplicationId,
                evt.RejectionReason);

            var notification = Notification.ForUser(evt.ApplicationOwnerId,"Application rejected", $"Application {evt.ApplicationId} rejected. Reason: {evt.RejectionReason}",NotificationType.MembershipApplicationRejected);
            await DispatchNotification(notification,ct);
        }
    }
}
