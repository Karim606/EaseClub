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
        public void Create_ShouldReturnError_WhenNumberOfRepeatsIsNonPositive()
        {
            // Act
            var result = RepeatRule.Create(0, RepeatMode.ExactValue);

            // Assert
            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(RepeatErrors.NonPositiveRepeatCount);
        }

        [Theory]
        [InlineData(3, 3, true)]  // 3 matches Exact 3
        [InlineData(2, 3, false)] // 2 does not match Exact 3
        public void Evaluate_ExactValueMode_ShouldValidateCorrectly(int actualInstances, int ruleCount, bool expected)
        {
            // Arrange
            var rule = RepeatRule.Create(ruleCount, RepeatMode.ExactValue).Value;

            // Act
            var result = rule.Evaluate(actualInstances);

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(1, 3, true)]  // 1 is within [1, 3]
        [InlineData(3, 3, true)]  // 3 is within [1, 3]
        [InlineData(0, 3, false)] // 0 is not >= 1
        [InlineData(4, 3, false)] // 4 is > 3
        public void Evaluate_AtLeastOneMode_ShouldValidateRange(int actualInstances, int ruleCount, bool expected)
        {
            // Arrange
            var rule = RepeatRule.Create(ruleCount, RepeatMode.AtLeastOne).Value;

            // Act
            var result = rule.Evaluate(actualInstances);

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void Evaluate_DefaultMode_ShouldReturnFalse()
        {
            // Arrange - Assuming a case not covered by the switch
            var rule = RepeatRule.Create(1, (RepeatMode)99).Value;

            // Act
            var result = rule.Evaluate(1);

            // Assert
            result.Should().BeFalse();
        }
    }
}
