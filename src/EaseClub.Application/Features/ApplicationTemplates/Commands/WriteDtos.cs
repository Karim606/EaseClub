using EaseClub.Application.Common.Dtos;
using EaseClub.Domain.ApplicationTemplates;
using EaseClub.Domain.ApplicationTemplates.SystemSections;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.ConditionExpression;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.RepeatRule;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.ValidationRulesSet;
using EaseClub.Domain.Common.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.ApplicationTemplates.Commands
{
    public record StepDetailsDto(
    Guid Id,
    string Title,
    string Category,
    List<SectionDetailsDto> Sections);

    public record SectionDetailsDto(
        Guid Id,
        string Title,
        SectionIntent Intent,
        List<FieldDetailsDto> Fields,
        RepeatRuleSetDto? RepeatRule=null );
    public record RepeatRuleSetDto(int NumberOfRepeats, RepeatMode Mode)
    {
        public Result<RepeatRule> ToDomain() => RepeatRule.Create(NumberOfRepeats, Mode);
    }

    public record FieldDetailsDto(
        Guid Id,
        string Key,
        string Label,
        FieldType FieldType,
        ValidationRuleSetDto ValidationRules,
        List<string>? AllowedValues = null// JSON string for low overhead
        )
        {
          public  FieldSpecification ToFieldSpecification() => new FieldSpecification(Key,FieldType,
              ValidationRules.ToDomain().IsSuccess ? ValidationRules.ToDomain().Value : null
              ,AllowedValues);
        }; // JSON logic for dynamic showing/hiding

    public record TemplateDetailsDto(
     Guid Id,
    string Name,
     List<StepDetailsDto> Steps
    );

}
