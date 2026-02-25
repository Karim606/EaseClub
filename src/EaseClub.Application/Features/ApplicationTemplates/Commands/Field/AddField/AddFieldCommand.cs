using EaseClub.Application.Common;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.ApplicationTemplates;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.ConditionExpression;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.ValidationRulesSet;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.ApplicationTemplates.Commands.Field.AddField
{
    public record AddFieldCommand(
        Guid ClubId,
        string Key,
        int Order,
        bool PersistToMembership,
        FieldType type,
        ValidationRuleSetDto ValidationRules,
        ConditionExpressionDto? VisibilityCondition
        ) : IRequest<Result<Guid>>, IRequireClubAdmin, IRequireClubOwnershipValidation
    {
        public Guid SectionId { get; init; }

        public IEnumerable<OwnershipRule> Rules()
        {
            yield return new OwnershipRule(
                auth => auth.CheckAppTemplateComponentsOwnership(typeof(ApplicationSectionDefinition), SectionId, ClubId),
                nameof(ApplicationSectionDefinition),
                SectionId
                );
        }
    }
}
