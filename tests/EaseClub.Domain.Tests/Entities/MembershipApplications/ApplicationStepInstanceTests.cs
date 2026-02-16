using EaseClub.Domain.MembershipApplications;
using EaseClub.Domain.MembershipApplications.Errors;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Tests.Entities.MembershipApplications
{
    public class ApplicationStepInstanceTests
    {
        private readonly Guid _appId = Guid.NewGuid();
        private readonly Guid _templateStepId = Guid.NewGuid();
        private readonly Guid _templateSectionId = Guid.NewGuid();

        [Fact]
        public void Create_ShouldReturnSuccess_WithDefaultStates()
        {
            // Act
            var result = ApplicationStepInstance.Create(Guid.NewGuid(), _appId, _templateStepId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.IsCompleted.Should().BeFalse();
            result.Value.IsLocked.Should().BeFalse();
            result.Value.Sections.Should().BeEmpty();
        }

        [Fact]
        public void AddNewSectionInstance_ShouldFail_WhenStepIsLocked()
        {
            // Arrange
            var step = ApplicationStepInstance.Create(Guid.NewGuid(), _appId, _templateStepId).Value;
            step.Lock();

            // Act
            var result = step.AddNewSectionInstance(_templateSectionId, 0);

            // Assert
            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(ApplicationStepInstanceErrors.StepLocked);
        }

        [Fact]
        public void AddNewSectionInstance_ShouldPreventDuplicateIndexForSameSection()
        {
            // Arrange
            var step = ApplicationStepInstance.Create(Guid.NewGuid(), _appId, _templateStepId).Value;
            step.AddNewSectionInstance(_templateSectionId, 0);

            // Act - Try adding index 0 again for the same template section
            var result = step.AddNewSectionInstance(_templateSectionId, 0);

            // Assert
            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(ApplicationStepInstanceErrors.DuplicateSectionInstance);
        }

        [Fact]
        public void AddNewSectionInstance_ShouldAllowSameIndexForDifferentSections()
        {
            // Arrange
            var step = ApplicationStepInstance.Create(Guid.NewGuid(), _appId, _templateStepId).Value;
            var differentSectionId = Guid.NewGuid();
            step.AddNewSectionInstance(_templateSectionId, 0);

            // Act - Index 0 is fine if it's for a different section definition
            var result = step.AddNewSectionInstance(differentSectionId, 0);

            // Assert
            result.IsSuccess.Should().BeTrue();
            step.Sections.Should().HaveCount(2);
        }

        [Fact]
        public void Complete_ShouldUpdateState_WhenNotLocked()
        {
            // Arrange
            var step = ApplicationStepInstance.Create(Guid.NewGuid(), _appId, _templateStepId).Value;

            // Act
            var result = step.Complete();

            // Assert
            result.IsSuccess.Should().BeTrue();
            step.IsCompleted.Should().BeTrue();
        }
    }
}
