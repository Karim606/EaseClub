using EaseClub.Application.Common;
using EaseClub.Domain.MembershipApplications;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipApplications.Commands.ReviewApplication
{
    public class ApplicationRejectedEventHandler
         : DomainEventHandler<ApplicationRejectedEvent, ApplicationRejectedEventHandler>
    {
        //private readonly INotificationService _notificationService;

        public ApplicationRejectedEventHandler(
            //INotificationService notificationService,
            ILogger<ApplicationRejectedEventHandler> logger
        ) : base(logger)
        {
            //_notificationService = notificationService;
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

            // Example future notification
            /*
            await _notificationService.SendAsync(
                evt.ApplicationId,
                $"Your membership application has been rejected. Reason: {evt.RejectionReason}"
            );
            */

            _logger.LogInformation(
                "Application {ApplicationId} rejected. Reason: {Reason}",
                evt.ApplicationId,
                evt.RejectionReason);
        }
    }
}
