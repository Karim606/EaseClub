using EaseClub.Domain.ApplicationTemplates;
using EaseClub.Domain.ApplicationTemplates.Errors;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.ConditionExpression;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.ValidationRulesSet;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Tests.Entities.ApplicationTemplates
{
    public class ApplicationFieldDefinitionTests
    {
        private readonly ValidationRuleSet _defaultRules = ValidationRuleSet.Create(isRequired: false).Value;

        [Fact]
        public void Create_ShouldReturnError_WhenKeyIsEmpty()
        {
            // Act
            var result = ApplicationFieldDefinition.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                 "",
                FieldType.Text,
                _defaultRules,
                null,
                false,
                false);

            // Assert
            result.IsError.Should().BeTrue();
            result.Errors.Should().Contain(ApplicationFieldErrors.KeyRequired);
        }

        [Fact]
        public void Create_ShouldReturnError_WhenSectionIdIsEmpty()
        {
            // Act
            var result = ApplicationFieldDefinition.Create(
                Guid.NewGuid(),
                Guid.Empty,
                "first_name",
                FieldType.Text,
                _defaultRules,
                null,
                false,
                false);

            // Assert
            result.IsError.Should().BeTrue();
            result.Errors.Should().Contain(ApplicationFieldErrors.SectionIdRequired);
        }

        [Fact]
        public void Validate_ShouldDelegateToStrategyRegistry()
        {
            // Arrange
            var rules = ValidationRuleSet.Create(isRequired: true, minLength: 5).Value;
            var field = ApplicationFieldDefinition.Create(
                Guid.NewGuid(), Guid.NewGuid(), "test_key",
                FieldType.Text, rules, null, false, false).Value;

            // Act
            var errors = field.Validate("abc"); // Too short

            // Assert
            errors.Should().NotBeEmpty();
            errors.Should().Contain(ValidationErrors.TooShort);
        }

        [Fact]
        public void Visible_ShouldReturnTrue_WhenNoConditionExists()
        {
            // Arrange
            var field = ApplicationFieldDefinition.Create(
                Guid.NewGuid(), Guid.NewGuid(), "test_key",
                FieldType.Text, _defaultRules, null, false, false).Value;

            // Act & Assert
            field.Visible("any value").Should().BeTrue();
        }

        [Theory]
        [InlineData("Yes", true)]
        [InlineData("No", false)]
        public void Visible_ShouldEvaluateCondition_WhenConditionExists(string dependentValue, bool expectedVisibility)
        {
            // Arrange: Field only visible if some other field is "Yes"
            var condition = ConditionExpression.Create("other_field", ComparisonOperator.Equals, "Yes").Value;
            var field = ApplicationFieldDefinition.Create(
                Guid.NewGuid(), Guid.NewGuid(), "test_key",
                FieldType.Text, _defaultRules, condition, false, false).Value;

            // Act
            var result = field.Visible(dependentValue);

            // Assert
            result.Should().Be(expectedVisibility);
        }
    }
}
