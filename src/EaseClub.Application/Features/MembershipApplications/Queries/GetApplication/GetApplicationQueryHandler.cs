using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
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
    ILogger<GetApplicationQueryHandler>logger)
    : IRequestHandler<GetApplicationQuery, Result<ApplicationResponse>>
    {
        public async Task<Result<ApplicationResponse>> Handle(GetApplicationQuery request, CancellationToken ct)
        {
            // 1. Fetch Application and its Answers in one trip
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
            app.Answers.Select(a => new AnswerDto(a.FieldDefinitionId, a.FieldKey, a.Value, a.InstanceIndex)).ToList(),
            app.GetPricePreview().Value
            );
        }
    }
}
