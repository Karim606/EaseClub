using EaseClub.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.ApplicationTemplates.ValueObjects.RepeatRule
{
    public static class RepeatErrors
    {
        public static Error FieldKeyRequired =>
            Error.Validation(
                code: "Repeat.FieldKey.Required",
                description: "Repeat rule requires a dependent field key."
            );

        public static Error InvalidMode =>
            Error.Validation(
                code: "Repeat.Mode.Invalid",
                description: "Repeat rule mode is invalid."
            );

        public static Error InvalidNumericValue =>
            Error.Validation(
                code: "Repeat.Value.InvalidNumeric",
                description: "Repeat rule requires a numeric field value."
            );

        public static Error NegativeRepeatCount =>
            Error.Validation(
                code: "Repeat.Count.Negative",
                description: "Repeat count cannot be negative."
            );

        public static Error RepeatRuleNotAllowed =>
            Error.Validation(
                code: "Section.Repeat.NotAllowed",
                description: "Repeat rule is not allowed for this section."
            );

        public static Error RepeatRuleRequired =>
            Error.Validation(
                code: "Section.Repeat.Required",
                description: "Repeat rule must be defined for repeatable section."
            );
    }
}
