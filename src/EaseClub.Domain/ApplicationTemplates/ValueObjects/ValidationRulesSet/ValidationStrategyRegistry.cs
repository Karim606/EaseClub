using EaseClub.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.ApplicationTemplates.ValueObjects.ValidationRulesSet
{
    public static class ValidationStrategyRegistry
    {
        private static readonly Dictionary< FieldType,List<IValidationStrategy>>Strategies = new()
        {
             {
                FieldType.Text,
                new List<IValidationStrategy>
                {
                    new RequiredValidationStrategy(),
                    new LengthValidationStrategy(),
                    new RegexValidationStrategy()
                }
             },

            {
                FieldType.Number,
                new List<IValidationStrategy>
                {
                new RequiredValidationStrategy(),
                new RangeValidationStrategy()
                }
            },

            {
                FieldType.Date,
                new List<IValidationStrategy>
                {
                new RequiredValidationStrategy(),
                new  DateRangeValidationStrategy()
                }
            },

            {
                FieldType.File,
                new List<IValidationStrategy>
                {
                new RequiredValidationStrategy()
                    // Later: FileSizeStrategy
                }
            },
            {
                FieldType.Enum,
                new List<IValidationStrategy>
                {
                new RequiredValidationStrategy()
                }
            }
        };

        public static List<Error> ApplyAll(string? value, ValidationRuleSet rules, FieldType type)
        {
            if (!Strategies.TryGetValue(type, out var strategies))
                return new();

              return strategies
            .Select(s => s.Validate(value, rules))
            .Where(e => e is not null)
            .Cast<Error>()
            .ToList();
        }
    }
}
