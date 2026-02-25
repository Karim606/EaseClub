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
    public class ApplicationStepTests
    {
        [Fact]
        public void AddNewSection_ShouldAddToList_WhenDataIsValid()
        {
            // Arrange
            var step = ApplicationStepDefinition.Create(
                Guid.NewGuid(), Guid.NewGuid(), "CAT", "Step", 0).Value;

            // Act
            var result = step.AddNewSection("Main Section", 0);

            // Assert
            result.IsSuccess.Should().BeTrue();
            step.Sections.Should().HaveCount(1);
            step.Sections[0].Title.Should().Be("Main Section");
        }

        [Fact]
        public void AddNewSection_ShouldShiftExistingSections()
        {
            // Arrange
            var step = ApplicationStepDefinition.Create(Guid.NewGuid(), Guid.NewGuid(), "CAT", "Step", 0).Value;
            step.AddNewSection("Old First", 0);

            // Act - Insert at 0
            step.AddNewSection("New First", 0);

            // Assert
            step.Sections.First(s => s.Title == "Old First").Order.Should().Be(1);
            step.Sections.First(s => s.Title == "New First").Order.Should().Be(0);
        }
    }
}
