using EaseClub.Domain.ApplicationTemplates;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;

namespace EaseClub.Domain.ApplicationTemplates.ValueObjects.ValidationRulesSet
{
    public record ValidationRuleSet
    {
        // Properties are init-only to maintain immutability
        public bool IsRequired { get; init; }
        public int? MinLength { get; init; }
        public int? MaxLength { get; init; }
        public string? Regex { get; init; }
        public decimal? MinValue { get; init; }
        public decimal? MaxValue { get; init; }

        // Private constructor prevents direct instantiation: new ValidationRuleSet(...)
        private ValidationRuleSet(bool isRequired, int? minLength, int? maxLength, string? regex, decimal? minValue, decimal? maxValue)
        {
            IsRequired = isRequired;
            MinLength = minLength;
            MaxLength = maxLength;
            Regex = regex;
            MinValue = minValue;
            MaxValue = maxValue;
        }

        public static Result<ValidationRuleSet> Create(
            bool isRequired = true,
            int? minLength = null,
            int? maxLength = null,
            string? regex = null,
            decimal? minValue = null,
            decimal? maxValue = null)
        {
            // 1. Domain Validation Logic
            if (minLength.HasValue && maxLength.HasValue && minLength > maxLength)
                return Error.Validation("Validation.Setup.Length", "Min length cannot be greater than Max length.");

            if (minValue.HasValue && maxValue.HasValue && minValue > maxValue)
                return Error.Validation("Validation.Setup.Range", "Min value cannot be greater than Max value.");

            // 2. Return valid instance
            return new ValidationRuleSet(isRequired, minLength, maxLength, regex, minValue, maxValue);
        }

        public List<Error> Validate(string? value, FieldType type)
        {
            return ValidationStrategyRegistry.ApplyAll(value, this, type);
        }
    }
}