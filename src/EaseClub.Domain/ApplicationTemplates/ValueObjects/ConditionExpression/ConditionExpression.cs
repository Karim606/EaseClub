using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipApplications.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.ApplicationTemplates.ValueObjects.ConditionExpression
{
    public record ConditionExpression
    {
        public string DependsOnFieldKey { get; init; }
        public ComparisonOperator Operator { get; init; }
        public string ExpectedValue { get; init; }

        private ConditionExpression() { }
        private ConditionExpression(string fieldKey, ComparisonOperator op, string value)
        {
            DependsOnFieldKey = fieldKey;
            Operator = op;
            ExpectedValue = value;
        }

        public static Result<ConditionExpression> Create(string fieldKey, ComparisonOperator op, string value)
        {
            if (string.IsNullOrWhiteSpace(fieldKey)) return ConditionErrors.FieldKeyRequired;
            if (string.IsNullOrWhiteSpace(value)) return ConditionErrors.ExpectedValueRequired;

            return new ConditionExpression(fieldKey, op, value);
        }

        public bool IsSatisfiedBy(string? actualValue)
        {
            return ComparisonStrategyRegistry.Evaluate(Operator, actualValue, ExpectedValue);
        }

        //ToSnapshot
        public ConditionExpressionSnapshot ToSnapshot()
        {
            return ConditionExpressionSnapshot.FromDomain(this);
        }
    }
}
