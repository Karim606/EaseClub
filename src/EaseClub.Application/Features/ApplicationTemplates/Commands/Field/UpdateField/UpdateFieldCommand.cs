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

namespace EaseClub.Application.Features.ApplicationTemplates.Commands.Field.UpdateField
{
    public record UpdateFieldCommand(
    Guid ClubId,
    string Label,
    bool PersistToMembership,
    ValidationRuleSetDto ValidationRules,
    ConditionExpressionDto? VisibilityCondition,
    List<string>?AllowedValues = null
) : IRequest<Result<Success>>, IRequireClubAdmin, IRequireClubOwnershipValidation
    {
        [JsonIgnore]
        public Guid FieldId { get; init; }

        public IEnumerable<OwnershipRule> Rules()
        {
            yield return new OwnershipRule(
                (auth, clubId) => auth.CheckAppTemplateComponentsOwnership(typeof(ApplicationFieldDefinition), FieldId, clubId),
                nameof(ApplicationFieldDefinition),
                FieldId);
        }
    }
}
