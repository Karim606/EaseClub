using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Features.ApplicationTemplates.Commands;
using EaseClub.Domain.ClubAdmin;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipApplications;
using EaseClub.Domain.MembershipApplications.Enums;
using EaseClub.Domain.MembershipApplications.Repositories;
using EaseClub.Domain.MembershipApplications.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace EaseClub.Application.Features.MembershipApplications.Queries.GetApplication
{

    public class GetApplicationQueryHandler(
    IMembershipApplicationRepository appRepo,
    ILogger<GetApplicationQueryHandler>logger)
    : IRequestHandler<GetApplicationQuery, Result<ApplicationUserResponse>>
    {
        public async Task<Result<ApplicationUserResponse>> Handle(GetApplicationQuery request, CancellationToken ct)
        {
            var app = await appRepo.GetByIdWithAnswersAsync(request.ApplicationId, ct);
            // ... validation checks ...

            if (app == null)
            {
                logger.LogWarning("The application was not found.");
                return Error.NotFound("Application.NotFound", "The application was not found.");
            }
            if (app.Status != ApplicationStatus.Draft) { logger.LogError("Conflict error in GetApplicationQueryHandler: {Error}", Error.Conflict("application isnt in draft status").ToLogObject()); return Error.Conflict("application isnt in draft status"); }
            var answers = app.Answers.ToDictionary(a => (a.FieldDefinitionId, a.InstanceId), a => a.Value);

            var steps = app.TemplateSnapshot.Steps
                .Select(step => MapStep(step, answers, app.Answers))
                .ToList();

            return new ApplicationUserResponse(app.Id, app.TrackingNumber, app.CurrentStepOrder, app.CompletedStepOrders.ToList(), steps);
        }

        // --- Break the complexity down into isolated, testable chunks ---

        private UserStepDto MapStep(StepSnapshot step, Dictionary<(Guid, string?), string?> answerDict, IEnumerable<EaseClub.Domain.MembershipApplications.ValueObjects.UserAnswer> allAnswers)
        {
            var sections = step.Sections.SelectMany(sec => MapSectionInstances(sec, answerDict, allAnswers)).ToList();
            return new UserStepDto(step.Id, step.Title, step.Order, sections);
        }

        private IEnumerable<UserSectionDto> MapSectionInstances(SectionSnapshot sec, Dictionary<(Guid, string?), string?> answerDict, IEnumerable<EaseClub.Domain.MembershipApplications.ValueObjects.UserAnswer> allAnswers)
        {
            var sectionFieldIds = sec.Fields.Select(f => f.Id).ToHashSet();
            var instanceIds = allAnswers.Where(a => sectionFieldIds.Contains(a.FieldDefinitionId))
                                            .Select(a => a.InstanceId)
                                            .Distinct()
                                            .ToList();

            if (!instanceIds.Any()) instanceIds.Add(null);

            return instanceIds.Select(id =>
                new UserSectionDto(sec.Id, sec.Title, sec.Intent, sec.RepeatRule, id, MapFields(sec.Fields, answerDict, id))
            );
        }

        private List<UserFieldDto> MapFields(IEnumerable<FieldSnapshot> fields, Dictionary<(Guid, string?), string?> answerDict, string? instanceId)
        {
            return fields.Select(f => {
                answerDict.TryGetValue((f.Id, instanceId), out var value);
                return new UserFieldDto(f.Id, f.Key, f.Label, f.FieldType, value, f.AllowedValues, f.ValidationRules);
            }).ToList();
        }
    }
}
