using EaseClub.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.ApplicationTemplates.ValueObjects.ConditionExpression
{
    public static class ConditionErrors
    {
        public static Error FieldKeyRequired =
            Error.Validation(code: "Condition.FieldKey.Required",
                description: "The field key that this condition depends on must be provided.");

        public static Error ExpectedValueRequired =
            Error.Validation(code: "Condition.ExpectedValue.Required",
                description: "An expected value must be provided to evaluate the condition.");

        public static Error InvalidNumericComparison =
            Error.Validation(code: "Condition.Numeric.Invalid",
                description: "The comparison operator requires a numeric value, but the provided expected value is not a valid number.");

        public static Error InvalidOperator =
            Error.Validation(code: "Condition.Operator.Unsupported",
                description: "The selected comparison operator is not supported by the current system version.");
    }
}
