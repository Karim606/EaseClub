using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.ApplicationTemplates.ValueObjects.ConditionExpression
{
    public static class ComparisonStrategyRegistry
    {
        private static readonly Dictionary<ComparisonOperator, IComparisonStrategy> Strategies;

        static ComparisonStrategyRegistry()
        {
            var allStrategies = new IComparisonStrategy[]
            {
                new EqualsStrategy(),
                new NotEqualsStrategy(),
                new GreaterThanStrategy(),
                new LessThanStrategy(),
                new GreaterThanOrEqualStrategy(),
                new LessThanOrEqualStrategy(),
                new ContainsStrategy(),
                new InStrategy()
            };

            Strategies = allStrategies.ToDictionary(s => s.Operator);
        }

        public static bool Evaluate(ComparisonOperator op, string? actual, string expected)
        {
            return Strategies.TryGetValue(op, out var strategy)
                && strategy.Evaluate(actual, expected);
        }
    }
}
