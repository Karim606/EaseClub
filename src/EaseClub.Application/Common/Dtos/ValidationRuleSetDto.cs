using EaseClub.Domain.ApplicationTemplates.ValueObjects.ValidationRulesSet;
using EaseClub.Domain.Common.Results;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Common.Dtos
{
    public record ValidationRuleSetDto(
    [DefaultValue(true)] bool IsRequired,
    int? MinLength = null,
    int? MaxLength = null,
    string? Regex = null,
    decimal? MinValue = null,
    decimal? MaxValue = null,
    DateTime? MinDate = null,
    DateTime? MaxDate = null
    )
    {
        public Result<ValidationRuleSet> ToDomain() => ValidationRuleSet.Create(IsRequired,
                MinLength,  MaxLength, Regex, MinValue, MaxValue);
        public static ValidationRuleSetDto FromDomain(ValidationRuleSet rule) => new(rule.IsRequired,rule.MinLength, rule.MaxLength, rule.Regex, rule.MinValue, 
            rule.MaxValue);
    }
}
