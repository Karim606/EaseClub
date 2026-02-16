using EaseClub.Domain.ApplicationTemplates.ValueObjects.ConditionExpression;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Tests.Entities.ApplicationTemplates.ValueObjects
{
    public class ConditionExpressionTests
    {
        [Fact]
        public void Create_ShouldReturnError_WhenFieldCodeIsEmpty()
        {
            // Act
            var result = ConditionExpression.Create("", ComparisonOperator.Equals, "Value");

            // Assert
            result.IsError.Should().BeTrue();
            result.Errors.Should().Contain(ConditionErrors.FieldCodeRequired);
        }

        [Theory]
        [InlineData(ComparisonOperator.Equals, "TrueValue", "TrueValue", true)]
        [InlineData(ComparisonOperator.Equals, "TrueValue", "FalseValue", false)]
        [InlineData(ComparisonOperator.NotEquals, "A", "B", true)]
        [InlineData(ComparisonOperator.GreaterThan, "10", "5", true)]
        [InlineData(ComparisonOperator.GreaterThan, "5", "10", false)]
        [InlineData(ComparisonOperator.LessThan, "5", "10", true)]
        [InlineData(ComparisonOperator.Contains, "Hello World", "Hello", true)]
        [InlineData(ComparisonOperator.In, "Gold", "Silver, Gold, Platinum", true)]
        [InlineData(ComparisonOperator.In, "Bronze", "Silver, Gold, Platinum", false)]
        public void IsSatisfiedBy_ShouldReturnExpectedResult_ForVariousOperators(
            ComparisonOperator op,
            string? actual,
            string expected,
            bool expectedResult)
        {
            // Arrange
            var condition = ConditionExpression.Create("some_field", op, expected).Value;

            // Act
            var result = condition.IsSatisfiedBy(actual);

            // Assert
            result.Should().Be(expectedResult);
        }

        [Fact]
        public void IsSatisfiedBy_ShouldBeCaseInsensitive_ForEquals()
        {
            // Arrange
            var condition = ConditionExpression.Create("field", ComparisonOperator.Equals, "MALE").Value;

            // Act & Assert
            condition.IsSatisfiedBy("male").Should().BeTrue();
        }

        [Theory]
        [InlineData("not_a_number", "10")]
        [InlineData("10", "not_a_number")]
        public void NumericOperators_ShouldReturnFalse_WhenParsingFails(string actual, string expected)
        {
            // Arrange
            var condition = ConditionExpression.Create("field", ComparisonOperator.GreaterThan, expected).Value;

            // Act
            var result = condition.IsSatisfiedBy(actual);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void InStrategy_ShouldHandleWhitespaceAndEmptyEntries()
        {
            // Arrange
            var strategy = new InStrategy();
            string expected = " OptionA ,  OptionB,OptionC ";

            // Act & Assert
            strategy.Evaluate("OptionA", expected).Should().BeTrue();
            strategy.Evaluate("OptionB", expected).Should().BeTrue();
            strategy.Evaluate("OptionD", expected).Should().BeFalse();
        }
    }
}
