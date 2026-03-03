using EaseClub.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EaseClub.Domain.ApplicationTemplates.ValueObjects.ValidationRulesSet
{
    public sealed class RequiredValidationStrategy : IValidationStrategy
    {
        public Error? Validate(string? value, ValidationRuleSet rules)
        {
            if (!rules.IsRequired)
                return null;

            return string.IsNullOrWhiteSpace(value)
                ? ValidationErrors.ValueRequired
                : null;
        }
    }

    public sealed class LengthValidationStrategy : IValidationStrategy
    {
        public Error? Validate(string? value, ValidationRuleSet rules)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (rules.MinLength.HasValue && value.Length < rules.MinLength.Value)
                return ValidationErrors.TooShort;

            if (rules.MaxLength.HasValue && value.Length > rules.MaxLength.Value)
                return ValidationErrors.TooLong;

            return null;
        }
    }

    public sealed class RangeValidationStrategy : IValidationStrategy
    {
        public Error? Validate(string? value, ValidationRuleSet rules)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (!decimal.TryParse(value, out var number))
                return ValidationErrors.InvalidFormat;

            if (rules.MinValue.HasValue && number < rules.MinValue.Value)
                return ValidationErrors.OutOfRange;

            if (rules.MaxValue.HasValue && number > rules.MaxValue.Value)
                return ValidationErrors.OutOfRange;

            return null;
        }
    }

    public sealed class DateRangeValidationStrategy : IValidationStrategy
    {
        public Error? Validate(string? value, ValidationRuleSet rules)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (!DateTime.TryParse(value, out var date))
                return ValidationErrors.InvalidFormat;

            if (rules.MinDate.HasValue && date < rules.MinDate.Value)
                return ValidationErrors.OutOfRange;

            if (rules.MaxDate.HasValue && date > rules.MaxDate.Value)
                return ValidationErrors.OutOfRange;

            return null;
        }
    }


    public sealed class RegexValidationStrategy : IValidationStrategy
    {
        public Error? Validate(string? value, ValidationRuleSet rules)
        {
            if (string.IsNullOrWhiteSpace(rules.Regex) ||
                string.IsNullOrWhiteSpace(value))
                return null;

            try
            {
                var isMatch = Regex.IsMatch(
                    value,
                    rules.Regex,
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
                    TimeSpan.FromMilliseconds(100));

                return isMatch
                    ? null
                    : ValidationErrors.InvalidFormat;
            }
            catch (RegexMatchTimeoutException)
            {
                return Error.Failure(
                    "Validation.Regex.Timeout",
                    "The validation process timed out.");
            }
            catch
            {
                return ValidationErrors.RegexInvalid;
            }
        }
    }
}
