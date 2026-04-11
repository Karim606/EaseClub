using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.ClubAdmin;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipApplications;
using EaseClub.Domain.MembershipApplications.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipApplications.Queries.GetApplication
{

    public class GetApplicationQueryHandler(
    IMembershipApplicationRepository appRepo,
    ICurrentUserService currentUserService,
    IClubAuthorizationService clubAuthorizationService,
    IClubAdminUserRepository clubAdminUserRepository,
    ILogger<GetApplicationQueryHandler>logger)
    : IRequestHandler<GetApplicationQuery, Result<ApplicationResponse>>
    {
        public async Task<Result<ApplicationResponse>> Handle(GetApplicationQuery request, CancellationToken ct)
        {
            var res = Guid.TryParse(currentUserService.GetId(), out var userId);

            if (res == false) return Error.Unauthorized();

            var roles = currentUserService.GetRoles();

            if (roles.Contains("ClubAdmin"))
            {
                var clubAdminUser = await clubAdminUserRepository.GetByIdAsync(userId, ct);
                var res2 = await clubAuthorizationService.DoesResourceBelongToClubAsync<MembershipApplication>(request.ApplicationId, clubAdminUser.ClubId);
                if (res2 == false)
                {
                    logger.LogWarning($"The application does not belong to the club {clubAdminUser.ClubId}.");
                    return Error.NotFound("Application.NotFound", "The application does not belong to the club.");
                }
            }

            else if (roles.Contains("Member"))
            {
                var res2 = await clubAuthorizationService.DoesResourceBelongToUserAsync<MembershipApplication>(request.ApplicationId, userId);
                if (res2 == false)
                {
                    logger.LogWarning($"The application does not belong to the user {userId}.");
                    return Error.NotFound("Application.NotFound", "The application does not belong to the user.");
                }
            }


            // 2. Fetch Application and its Answers in one trip
            var app = await appRepo.GetByIdWithAnswersAsync(request.ApplicationId, ct);

            if (app == null)
            {
                logger.LogWarning("The application was not found.");
                return Error.NotFound("Application.NotFound", "The application was not found.");
            }


            return new ApplicationResponse(
            app.Id,
            app.TrackingNumber,
            app.CurrentStepOrder,
            app.CompletedStepOrders.ToList(),
            app.TemplateSnapshot,
            app.Answers.Select(a => new AnswerDto(a.FieldDefinitionId, a.FieldKey,a.FieldType, a.Value, a.InstanceIndex)).ToList(),
            app.GetPricePreview().Value
            );
        }
    }
}
