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
            var name = "Standard Plan";

            // Act - Aligned with (id, clubId, name)
            var result = ApplicationTemplateDefinition.Create(id, clubId, name);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Name.Should().Be(name);
            result.Value.Steps.Should().BeEmpty();
        }

        [Fact]
        public void AddNewStep_ShouldAddStep_WhenDataIsValid()
        {
            // Arrange
            var template = ApplicationTemplateDefinition.Create(Guid.NewGuid(), Guid.NewGuid(), "Plan").Value;

            // Act
            var result = template.AddNewStep("Identity", "Identification Details", 0);

            // Assert
            result.IsSuccess.Should().BeTrue();
            template.Steps.Should().HaveCount(1);
            template.Steps[0].Title.Should().Be("Identification Details");
        }

        [Fact]
        public void RemoveStep_ShouldSucceed_AndCloseGap()
        {
            // Arrange
            var template = ApplicationTemplateDefinition.Create(Guid.NewGuid(), Guid.NewGuid(), "Plan").Value;
            template.AddNewStep("Cat", "Step 0", 0);
            var step1 = template.AddNewStep("Cat", "Step 1", 1).Value;
            template.AddNewStep("Cat", "Step 2", 2);

            // Act
            var result = template.RemoveStep(step1.Id);

            // Assert
            result.IsSuccess.Should().BeTrue();
            template.Steps.Should().HaveCount(2);
            template.Steps.First(s => s.Title == "Step 2").Order.Should().Be(1); // Shifted down
        }
    }
}
