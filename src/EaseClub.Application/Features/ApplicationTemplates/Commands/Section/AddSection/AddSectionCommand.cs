using EaseClub.Application.Common;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.ApplicationTemplates;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.RepeatRule;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.ApplicationTemplates.Commands.Section.AddSection
{
    public record AddSectionCommand(
    Guid ClubId,
    string Title,
    int Order,
    RepeatRuleDto? RepeatRuleJson) : IRequest<Result<Guid>>, IRequireClubAdmin, IRequireClubOwnershipValidation
    {
        [JsonIgnore]
        public Guid StepId { get; init; }
        public IEnumerable<OwnershipRule> Rules()
        {
            yield return new OwnershipRule(
                auth => auth.CheckAppTemplateComponentsOwnership(typeof(ApplicationStepDefinition), StepId, ClubId),
                nameof(ApplicationStepDefinition),
                StepId
                );
        }
    }

}
