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
        public void Create_ShouldReturnSuccess_WhenDataIsValid()
        {
            // Arrange
            var id = Guid.NewGuid();
            var templateId = Guid.NewGuid();
            var category = "IDENTITY";
            var title = "Personal Details";
            var order = 1;

            // Act - Accessible via InternalsVisibleTo
            var result = ApplicationStepDefinition.Create(id, templateId, category, title, order);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Title.Should().Be(title);
            result.Value.Category.Should().Be(category);
            result.Value.Sections.Should().BeEmpty();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Create_ShouldFail_WhenOrderIsInvalid(int invalidOrder)
        {
            // Act
            var result = ApplicationStepDefinition.Create(
                Guid.NewGuid(), Guid.NewGuid(), "CAT", "Title", invalidOrder);

            // Assert
            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(ApplicationStepErrors.InvalidOrder);
        }

        [Fact]
        public void AddNewSection_ShouldAddToList_WhenDataIsValid()
        {
            // Arrange
            var step = ApplicationStepDefinition.Create(
                Guid.NewGuid(), Guid.NewGuid(), "CAT", "Step", 1).Value;

            // Act
            var result = step.AddNewSection("Main Section", 1);

            // Assert
            result.IsSuccess.Should().BeTrue();
            step.Sections.Should().HaveCount(1);
            step.Sections[0].Title.Should().Be("Main Section");
        }

        [Fact]
        public void AddNewSection_ShouldPreventDuplicateTitles_CaseInsensitive()
        {
            // Arrange
            var step = ApplicationStepDefinition.Create(
                Guid.NewGuid(), Guid.NewGuid(), "CAT", "Step", 1).Value;
            step.AddNewSection("Profile", 1);

            // Act
            var result = step.AddNewSection("profile", 2);

            // Assert
            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(ApplicationStepErrors.DuplicateSectionTitle);
        }

        [Fact]
        public void RemoveSection_ShouldSucceed_WhenSectionExists()
        {
            // Arrange
            var step = ApplicationStepDefinition.Create(
                Guid.NewGuid(), Guid.NewGuid(), "CAT", "Step", 1).Value;
            var section = step.AddNewSection("Delete Me", 1).Value;

            // Act
            var result = step.RemoveSection(section.Id);

            // Assert
            result.IsSuccess.Should().BeTrue();
            step.Sections.Should().BeEmpty();
        }
    }
}
