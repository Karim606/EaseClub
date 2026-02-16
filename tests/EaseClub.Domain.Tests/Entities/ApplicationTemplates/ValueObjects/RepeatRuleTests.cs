using EaseClub.Domain.ApplicationTemplates.ValueObjects.RepeatRule;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Tests.Entities.ApplicationTemplates.ValueObjects
{
    public class RepeatRuleTests
    {
        [Fact]
        public void Create_ShouldReturnError_WhenFieldCodeIsEmpty()
        {
            // Act
            var result = RepeatRule.Create("", RepeatMode.ExactValue);

            // Assert
            result.IsError.Should().BeTrue();
            result.Errors.Should().Contain(RepeatErrors.FieldCodeRequired);
        }

        [Theory]
        [InlineData("3", 3)]
        [InlineData("0", 0)]
        [InlineData("-5", -5)] // Domain check: does your UI handle negative repeats?
        [InlineData("not_a_number", 0)]
        public void Evaluate_ExactValueMode_ShouldReturnParsedNumber(string input, int expected)
        {
            // Arrange
            var rule = RepeatRule.Create("children_count", RepeatMode.ExactValue).Value;

            // Act
            var result = rule.Evaluate(input);

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData("0", 1)] // Should bump 0 up to 1
        [InlineData("3", 3)] // Should stay 3
        [InlineData("not_a_number", 0)] // Parse failure usually returns 0 in your code
        public void Evaluate_AtLeastOneMode_ShouldReturnMinimumOfOne(string input, int expected)
        {
            // Arrange
            var rule = RepeatRule.Create("guest_count", RepeatMode.AtLeastOne).Value;

            // Act
            var result = rule.Evaluate(input);

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void Evaluate_NoneMode_ShouldAlwaysReturnZero()
        {
            // Arrange
            var rule = RepeatRule.Create("any_field", RepeatMode.None).Value;

            // Act
            var result = rule.Evaluate("99");

            // Assert
            result.Should().Be(0);
        }

        [Fact]
        public void Evaluate_ShouldReturnZero_WhenInputIsNull()
        {
            // Arrange
            var rule = RepeatRule.Create("field", RepeatMode.ExactValue).Value;

            // Act
            var result = rule.Evaluate(null);

            // Assert
            result.Should().Be(0);
        }
    }
}
