using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.ApplicationTemplates.ValueObjects.ConditionExpression
{

    internal static class ComparisonHelpers
    {
        public static bool TryParseNumbers(string? actual, string expected, out decimal a, out decimal e)
        {
            var parsedActual = decimal.TryParse(actual, out a);
            var parsedExpected = decimal.TryParse(expected, out e);

            return parsedActual && parsedExpected;
        }
    }

    public sealed class EqualsStrategy : IComparisonStrategy
    {
        public ComparisonOperator Operator => ComparisonOperator.Equals;

        public bool Evaluate(string? actual, string expected) =>
            actual?.Equals(expected, StringComparison.OrdinalIgnoreCase) ?? false;
    }


    public sealed class GreaterThanStrategy : IComparisonStrategy
    {
        public ComparisonOperator Operator => ComparisonOperator.GreaterThan;

        public bool Evaluate(string? actual, string expected)
        {
            return ComparisonHelpers.TryParseNumbers(actual, expected, out var a, out var e)
                && a > e;
        }
    }

    public sealed class NotEqualsStrategy : IComparisonStrategy
    {
        public ComparisonOperator Operator => ComparisonOperator.NotEquals;

        public bool Evaluate(string? actual, string expected) =>
            !(actual?.Equals(expected, StringComparison.OrdinalIgnoreCase) ?? false);
    }

    public sealed class LessThanStrategy : IComparisonStrategy
    {
        public ComparisonOperator Operator => ComparisonOperator.LessThan;

        public bool Evaluate(string? actual, string expected)
        {
            return ComparisonHelpers.TryParseNumbers(actual, expected, out var a, out var e)
                && a < e;
        }
    }

    public sealed class GreaterThanOrEqualStrategy : IComparisonStrategy
    {
        public ComparisonOperator Operator => ComparisonOperator.GreaterThanOrEqual;

        public bool Evaluate(string? actual, string expected)
        {
            return ComparisonHelpers.TryParseNumbers(actual, expected, out var a, out var e)
                && a >= e;
        }
    }

    public sealed class LessThanOrEqualStrategy : IComparisonStrategy
    {
        public ComparisonOperator Operator => ComparisonOperator.LessThanOrEqual;

        public bool Evaluate(string? actual, string expected)
        {
            return ComparisonHelpers.TryParseNumbers(actual, expected, out var a, out var e)
                && a <= e;
        }

    }

    public sealed class ContainsStrategy : IComparisonStrategy
    {
        public ComparisonOperator Operator => ComparisonOperator.Contains;

        public bool Evaluate(string? actual, string expected) =>
            actual?.Contains(expected, StringComparison.OrdinalIgnoreCase) ?? false;
    }

    public sealed class InStrategy : IComparisonStrategy
    {
        public ComparisonOperator Operator => ComparisonOperator.In;

        public bool Evaluate(string? actual, string expected)
        {
            if (string.IsNullOrWhiteSpace(actual))
                return false;

            var options = expected.Split(
                ',',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            return options.Contains(actual, StringComparer.OrdinalIgnoreCase);
        }
    }
}
