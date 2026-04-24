using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Features.MembershipApplications.Queries.GetApplication;
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
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipApplications.Queries.GetApplicationForAdmin
{
    public class GetApplicationAdminQueryHandler(
    IMembershipApplicationRepository appRepo,
    ILogger<GetApplicationQueryHandler> logger)
: IRequestHandler<GetApplicationAdminQuery, Result<ApplicationAdminResponse>>
    {
        public async Task<Result<ApplicationAdminResponse>> Handle(GetApplicationAdminQuery request, CancellationToken ct)
        {

            var app = await appRepo.GetByIdWithAnswersAsync(request.ApplicationId, ct);
            // ... validation checks ...

            if (app == null)
            {
                logger.LogWarning("The application was not found.");
                return Error.NotFound("Application.NotFound", "The application was not found.");
            }
            if (app.Status == ApplicationStatus.Draft) { logger.LogError("NotFound error in GetApplicationAdminQueryHandler: {Error}", Error.NotFound( ).ToLogObject()); return Error.NotFound( ); }
            var answers = app.Answers.ToDictionary(a => (a.FieldDefinitionId, a.InstanceIndex), a => a.Value);

            var steps = app.TemplateSnapshot.Steps
                .Select(step => MapStep(step, answers, app.Answers))
                .ToList();
            
            return new ApplicationAdminResponse(app.Id, app.TrackingNumber,app.Status,steps);
        }

        // --- Break the complexity down into isolated, testable chunks ---

        private AdminStepDto MapStep(StepSnapshot step, Dictionary<(Guid, int), string> answerDict, IEnumerable<ApplicationAnswer> allAnswers)
        {
            var sections = step.Sections.SelectMany(sec => MapSectionInstances(sec, answerDict, allAnswers)).ToList();
            return new AdminStepDto(step.Id, step.Title, step.Order, sections);

        }

        private IEnumerable<AdminSectionDto> MapSectionInstances(SectionSnapshot sec, Dictionary<(Guid, int), string> answerDict, IEnumerable<ApplicationAnswer> allAnswers)
        {
            var sectionFieldIds = sec.Fields.Select(f => f.Id).ToHashSet();
            var instanceIndexes = allAnswers.Where(a => sectionFieldIds.Contains(a.FieldDefinitionId))
                                            .Select(a => a.InstanceIndex)
                                            .Distinct()
                                            .ToList();

            if (!instanceIndexes.Any()) instanceIndexes.Add(0);

            return instanceIndexes.OrderBy(i => i).Select(index =>
                new AdminSectionDto(sec.Id, sec.Title, sec.Intent, sec.RepeatRule, index, MapFields(sec.Fields, answerDict, index))
            );
        }

        private List<AdminFieldDto> MapFields(IEnumerable<FieldSnapshot> fields, Dictionary<(Guid, int), string> answerDict, int instanceIndex)
        {
            return fields.Select(f => {
                answerDict.TryGetValue((f.Id, instanceIndex), out var value);
                return new AdminFieldDto(f.Id, f.Key, f.Label, f.Type, value, f.AllowedValues, f.ValidationRules);
            }).ToList();
        }
    }
}
