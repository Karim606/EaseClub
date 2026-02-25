using EaseClub.Domain.ApplicationTemplates;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.ConditionExpression;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.RepeatRule;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.ValidationRulesSet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.ApplicationTemplates.Queries.GetStepByOrder
{
    public record StepDetailsDto(
    Guid Id,
    string Title,
    string Category,
    int Order,
    List<SectionDetailsDto> Sections);

    public record SectionDetailsDto(
        Guid Id,
        string Title,
        bool IsRepeatable,
        int Order,
        List<FieldDetailsDto> Fields,
        RepeatRule? RepeatRule=null
      );

    public record FieldDetailsDto(
        Guid Id,
        string Key,
        FieldType FieldType,
        bool IsRequired,
        ValidationRuleSet? ValidationRules, // JSON string for low overhead
        ConditionExpression? VisibilityConditions); // JSON logic for dynamic showing/hiding

    public record TemplateDetailsDto(
     Guid Id,
    string Name,
     List<StepDetailsDto> Steps
    );
}
