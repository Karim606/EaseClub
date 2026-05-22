using EaseClub.Domain.ApplicationTemplates;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.ValidationRulesSet;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Tests.Entities.ApplicationTemplates.ValueObjects
{
    public class ValidationRuleSetTests
    {
        #region Factory Tests

        [Fact]
        public void Create_ShouldReturnError_WhenMinLengthGreaterThanMaxLength()
        {
            // Act
            var result = ValidationRuleSet.Create(isRequired: true, minLength: 10, maxLength: 5);

            // Assert
            result.IsError.Should().BeTrue();
            result.Errors.Any(e => e.Code == "Validation.Setup.Length").Should().BeTrue();
        }

        [Fact]
        public void Create_ShouldReturnError_WhenMinValueGreaterThanMaxValue()
        {
            // Act
            var result = ValidationRuleSet.Create(isRequired: false, minValue: 100, maxValue: 50);

            // Assert
            result.IsError.Should().BeTrue();
            result.Errors.Any(e => e.Code == "Validation.Setup.Range").Should().BeTrue();
        }

        #endregion

        #region Strategy Tests - Text

        [Theory]
        [InlineData("", false)] // Required error
        [InlineData("abc", false)] // Too short (min 5)
        [InlineData("abcdefghijk", false)] // Too long (max 10)
        [InlineData("abcde", true)] // Perfect
        public void Validate_TextType_ShouldApplyLengthAndRequiredRules(string? value, bool shouldBeValid)
        {
            // Arrange
            var rules = ValidationRuleSet.Create(isRequired: true, minLength: 5, maxLength: 10).Value;

            // Act
            var errors = rules.Validate(value, FieldType.Text);

            // Assert
            if (shouldBeValid)
                errors.Should().BeEmpty();
            else
                errors.Should().NotBeEmpty();
        }
        #endregion

        #region Strategy Tests - Numbers

        [Theory]
        [InlineData("50", true)]
        [InlineData("5", false)]  // Below min 10
        [InlineData("150", false)] // Above max 100
        [InlineData("not_a_number", false)]
        public void Validate_NumberType_ShouldApplyRangeRules(string value, bool shouldBeValid)
        {
            // Arrange
            var rules = ValidationRuleSet.Create(isRequired: true, minValue: 10, maxValue: 100).Value;

            // Act
            var errors = rules.Validate(value, FieldType.Number);

            // Assert
            if (shouldBeValid)
                errors.Should().BeEmpty();
            else
                errors.Should().NotBeEmpty();
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void Validate_ShouldReturnEmpty_WhenFieldTypeHasNoStrategies()
        {
            // Arrange
            var rules = ValidationRuleSet.Create(isRequired: true).Value;

            // Act
            // Assuming we pass a FieldType not in the dictionary if that were possible, 
            // otherwise testing a type that only has 'Required'
            var errors = rules.Validate("some value", FieldType.Enum);

            // Assert
            errors.Should().BeEmpty();
        }

        #endregion
    }
}
