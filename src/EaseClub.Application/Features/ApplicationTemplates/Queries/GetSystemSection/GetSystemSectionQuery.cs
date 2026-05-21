using EaseClub.Application.Common.Dtos;
using EaseClub.Domain.ApplicationTemplates;
using EaseClub.Domain.ApplicationTemplates.SystemSections;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.ApplicationTemplates.Queries.GetSystemSection
{
    public record GetSystemSectionQuery(SectionIntent Intent) : IRequest<Result<SystemSectionDto>>;

    public record SystemSectionDto(
        string Title,
        SectionIntent Intent,
        List<SystemFieldDto> Fields
   )
    {
        public static SystemSectionDto FromDomain(SystemSectionDefinition sec) {
           return  new SystemSectionDto(sec.Title, sec.Intent,
            sec.Fields.Select(f=>SystemFieldDto.FromDomain(f)).ToList());

        }
    }

    public record SystemFieldDto(
        string Key,
        string Label,
        FieldType Type,
        ValidationRuleSetDto RuleSet,
        OverridenValidationRules ValidationRulesCanBeOverriden,
        List<string>? AllowedValues = null
    )
    {
        public static SystemFieldDto FromDomain(SystemFieldDefinition field)
        {
            return new SystemFieldDto(field.Key, field.Label, field.Type, ValidationRuleSetDto.FromDomain(field.RuleSet), field.ValidationRulesCanBeOverriden, field.AllowedValues);

        }
    }
}
