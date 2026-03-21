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
        public void Create_ShouldReturnError_WhenSectionIdIsEmpty()
        {
            // Act
            var result = ApplicationFieldDefinition.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.Empty, // Invalid
                "first_name",
                "fieldLabel",
                FieldType.Text,
                _defaultRules,
                null,
                false,
                0); // Fixed: changed false to 0 (int order)

            // Assert
            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(ApplicationFieldErrors.SectionIdRequired);
        }

        [Fact]
        public void Create_ShouldReturnError_WhenTemplateIdIsEmpty()
        {
            // Act
            var result = ApplicationFieldDefinition.Create(
                Guid.NewGuid(),
                Guid.Empty, // Invalid TemplateId
                Guid.NewGuid(),
                "first_name",
                "First Name",
                FieldType.Text,
                _defaultRules,
                null,
                false,
                1);

            // Assert
            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(ApplicationFieldErrors.TemplateIdRequired);
        }

        [Fact]
        public void Create_ShouldReturnError_WhenTypeIsEnumAndAllowedValuesAreEmpty()
        {
            // Act
            var result = ApplicationFieldDefinition.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                "dropdown_field",
                "Select Option",
                FieldType.Enum, // Type is Enum
                _defaultRules,
                null,
                false,
                0,
                allowedValues: new List<string>()); // Empty list

            // Assert
            result.IsError.Should().BeTrue();
            result.TopError.Description.Should().Contain("Allowed values are required");
        }

        [Fact]
        public void Create_ShouldCleanAndSetAllowedValues_WhenTypeIsEnum()
        {
            // Arrange
            var rawValues = new List<string> { " Red ", "blue", "RED", " Green " };

            // Act
            var result = ApplicationFieldDefinition.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                "color_pick",
                "Pick Color",
                FieldType.Enum,
                _defaultRules,
                null,
                false,
                0,
                allowedValues: rawValues);

            // Assert
            result.IsError.Should().BeFalse();
            // Should be: trimmed, unique, and case-sensitive preservation is tricky 
            // based on your private SetAllowedValues logic. Currently, .Distinct() 
            // is case-sensitive. If you want case-insensitive, see the logic below.
            result.Value.AllowedValues.Should().HaveCount(3);
            result.Value.AllowedValues.Should().Contain(new[] { "Red", "blue", "Green" });
        }

        [Fact]
        public void Update_ShouldReturnError_WhenUpdatingEnumToHaveNoValues()
        {
            // Arrange: Start with a valid Enum field
            var field = ApplicationFieldDefinition.Create(
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                "test_key", "Label", FieldType.Enum, _defaultRules, null, false, 0,
                false,
                new List<string> { "Option 1" }).Value;

            // Act: Update with null/empty values
            var result = field.Update(
                "New Label",
                _defaultRules,
                null,
                false,
                allowedValues: new List<string>());

            // Assert
            result.IsError.Should().BeTrue();
            result.TopError.Description.Should().Contain("Enum fields require allowed values");
        }

    }
}
