using EaseClub.Application.Common;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Features.MembershipApplications.Queries;
using EaseClub.Domain.MembershipApplications;
using EaseClub.Domain.MembershipApplications.Repositories;
using EaseClub.Domain.Memberships;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipApplications.Commands.ReviewApplication
{
    public class ApplicationApprovedEventHandler : DomainEventHandler<ApplicationApprovedEvent,ApplicationApprovedEventHandler>
    {
        //private readonly INotificationService _notificationService;
        private readonly IMembershipApplicationRepository _appRepo;
        private readonly IMembershipRepository _membershipRepo;
        private readonly IUnitOfWork _unitOfWork;
        public ApplicationApprovedEventHandler(
            //INotificationService notificationService,
            IMembershipApplicationRepository appRepo,
            IMembershipRepository membershipRepo,
            IUnitOfWork unitOfWork,
            ILogger<ApplicationApprovedEventHandler> logger):base(logger)
        {
           // _notificationService = notificationService;
            _appRepo = appRepo;
            _membershipRepo = membershipRepo;
            _unitOfWork = unitOfWork;
        }

        protected override async Task HandleEvent(ApplicationApprovedEvent evt, CancellationToken ct)
        {
            // Send notification
            //await _notificationService.SendAsync(evt.ApplicationId, "Your application has been approved");

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
        }
    }
}
