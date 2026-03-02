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
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.ApplicationTemplates.Commands.Field.AddField
{
    public record AddFieldCommand(
        Guid ClubId,
        Guid TemplateId,
        string Key,
        string label,
        bool PersistToMembership,
        FieldType type,
        ValidationRuleSetDto ValidationRules,
        ConditionExpressionDto? VisibilityCondition,
        List<string>? AllowedValues = null
        ) : IRequest<Result<Guid>>, IRequireClubAdmin, IRequireClubOwnershipValidation
    {
        [JsonIgnore]
        public Guid SectionId { get; init; }
        public IEnumerable<OwnershipRule> Rules()
        {
            yield return new OwnershipRule(
                auth => auth.DoesResourceBelongToClubAsync<ApplicationTemplateDefinition>(TemplateId, ClubId),
                nameof(ApplicationTemplateDefinition),
                TemplateId
                );
        }
    }
}
