using EaseClub.Domain.Common.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.ApplicationTemplates.ValueObjects.ConditionExpression
{
    public record ConditionExpression
    {
        public string DependsOnFieldCode { get; init; }
        public ComparisonOperator Operator { get; init; }
        public string ExpectedValue { get; init; }

        private ConditionExpression(string fieldCode, ComparisonOperator op, string value)
        {
            DependsOnFieldCode = fieldCode;
            Operator = op;
            ExpectedValue = value;
        }

        public static Result<ConditionExpression> Create(string fieldCode, ComparisonOperator op, string value)
        {
            if (string.IsNullOrWhiteSpace(fieldCode)) return ConditionErrors.FieldCodeRequired;
            if (string.IsNullOrWhiteSpace(value)) return ConditionErrors.ExpectedValueRequired;

            return new ConditionExpression(fieldCode, op, value);
        }

        public bool IsSatisfiedBy(string? actualValue)
        {
            return ComparisonStrategyRegistry.Evaluate(Operator, actualValue, ExpectedValue);
        }
    }
}
