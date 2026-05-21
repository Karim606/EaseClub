using EaseClub.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.ApplicationTemplates.ValueObjects.ValidationRulesSet
{
    public static class ValidationErrors
    {
        public static Error RegexInvalid = Error.Validation("Validation.Regex.Invalid", "The provided regular expression is not valid.");
        public static Error MinValueGreaterThanMax = Error.Validation("Validation.Range.Invalid",
            "Minimum value cannot be greater than maximum value.");
        public static Error ValueRequired = Error.Validation("Validation.Value.Required", "This field is required.");
        public static Error TooShort = Error.Validation("Validation.String.TooShort", "The input is too short.");
        public static Error TooLong = Error.Validation("Validation.String.TooLong", "The input is too long.");
        public static Error InvalidFormat = Error.Validation("Validation.Format.Invalid", "The input format is incorrect.");
        public static Error OutOfRange = Error.Validation("Validation.Numeric.OutOfRange", "The value is outside the allowed range.");
    }
}
