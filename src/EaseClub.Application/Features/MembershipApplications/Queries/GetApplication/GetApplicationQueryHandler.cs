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
    IMembershipApplicationRepository repository,
    ILogger<GetApplicationQueryHandler>logger)
    : IRequestHandler<GetApplicationQuery, Result<ApplicationResponse>>
    {
        public async Task<Result<ApplicationResponse>> Handle(GetApplicationQuery request, CancellationToken ct)
        {
            // 1. Fetch Application and its Answers in one trip
            var application = await repository.GetByIdWithAnswersAsync(request.ApplicationId, ct);

            if (application == null)
            {
                logger.LogWarning("The application was not found.");
                return Error.NotFound("Application.NotFound", "The application was not found.");
            }
            // 2. Parse the Snapshot back into a dynamic object
            // We use JsonDocument to avoid creating a rigid DTO for the snapshot
            var structure = JsonSerializer.Deserialize<JsonElement>(application.TemplateSnapshot);

            // 3. Map the Answers
            var answers = application.Answers.Select(a => new AnswerDto(
                a.FieldDefinitionId,
                a.Value,
                a.InstanceIndex
            )).ToList();

            return new ApplicationResponse(
                application.Id,
                application.Status.ToString(),
                structure,
                answers
            );
        }
    }
}
