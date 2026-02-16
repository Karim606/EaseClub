using EaseClub.Domain.ApplicationTemplates;
using EaseClub.Domain.ApplicationTemplates.Errors;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Tests.Entities.ApplicationTemplates
{
    public class ApplicationTemplateTests
    {
        [Fact]
        public void Create_ShouldReturnSuccess_WhenDataIsValid()
        {
            // Arrange
            var id = Guid.NewGuid();
            var clubId = Guid.NewGuid();
            var memberTypeId = Guid.NewGuid();
            var name = "Standard Plan";

            // Act
            var result = ApplicationTemplateDefinition.Create(id, clubId, memberTypeId, null, name);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Name.Should().Be(name);
            result.Value.Steps.Should().BeEmpty();
        }

        [Fact]
        public void AddNewStep_ShouldAddStep_WhenDataIsValid()
        {
            // Arrange
            var template = ApplicationTemplateDefinition.Create(
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, "Plan").Value;

            // Act
            var result = template.AddNewStep("Identity", "Identification Details", 1);

            // Assert
            result.IsSuccess.Should().BeTrue();
            template.Steps.Should().HaveCount(1);
            template.Steps[0].Title.Should().Be("Identification Details");
        }

        [Fact]
        public void AddNewStep_ShouldPreventDuplicateTitles()
        {
            // Arrange
            var template = ApplicationTemplateDefinition.Create(
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, "Plan").Value;
            template.AddNewStep("Cat", "Common Title", 1);

            // Act
            var result = template.AddNewStep("Cat", "Common Title", 2);

            // Assert
            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(ApplicationTemplateDefinitionErrors.DuplicateStepTitle);
        }

        [Fact]
        public void RemoveStep_ShouldSucceed_WhenStepExists()
        {
            // Arrange
            var template = ApplicationTemplateDefinition.Create(
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, "Plan").Value;
            var step = template.AddNewStep("Cat", "Title", 1).Value;

            // Act
            var result = template.RemoveStep(step.Id);

            // Assert
            result.IsSuccess.Should().BeTrue();
            template.Steps.Should().BeEmpty();
        }
    }
}
