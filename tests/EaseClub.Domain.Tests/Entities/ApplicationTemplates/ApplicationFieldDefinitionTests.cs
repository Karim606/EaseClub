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
    }
}
