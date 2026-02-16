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
    public class ApplicationSectionInstanceTests
    {
        private readonly Guid _stepInstanceId = Guid.NewGuid();
        private readonly Guid _templateSectionId = Guid.NewGuid();
        private readonly Guid _applicationId = Guid.NewGuid();

        [Fact]
        public void Create_ShouldReturnSuccess_WhenDataIsValid()
        {
            // Act - Testing internal factory
            var result = ApplicationSectionInstance.Create(
                Guid.NewGuid(),
                _stepInstanceId,
                _templateSectionId,
                index: 0);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Index.Should().Be(0);
            result.Value.Fields.Should().BeEmpty();
        }

        [Fact]
        public void Create_ShouldFail_WhenIndexIsNegative()
        {
            // Act
            var result = ApplicationSectionInstance.Create(
                Guid.NewGuid(), _stepInstanceId, _templateSectionId, -1);

            // Assert
            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(ApplicationSectionInstanceErrors.InvalidIndex);
        }

        [Fact]
        public void AddNewFieldValue_ShouldAddToList_WhenFieldIsUnique()
        {
            // Arrange
            var sectionInstance = ApplicationSectionInstance.Create(
                Guid.NewGuid(), _stepInstanceId, _templateSectionId, 0).Value;
            var fieldDefId = Guid.NewGuid();

            // Act
            var result = sectionInstance.AddNewFieldValue(_applicationId, fieldDefId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            sectionInstance.Fields.Should().HaveCount(1);
            sectionInstance.Fields[0].FieldDefinitionId.Should().Be(fieldDefId);
        }

        [Fact]
        public void AddNewFieldValue_ShouldFail_WhenFieldDefinitionIdIsDuplicate()
        {
            // Arrange
            var sectionInstance = ApplicationSectionInstance.Create(
                Guid.NewGuid(), _stepInstanceId, _templateSectionId, 0).Value;
            var fieldDefId = Guid.NewGuid();

            // Add the first field
            sectionInstance.AddNewFieldValue(_applicationId, fieldDefId);

            // Act - Try adding the same field definition again
            var result = sectionInstance.AddNewFieldValue(_applicationId, fieldDefId);

            // Assert
            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(ApplicationSectionInstanceErrors.DuplicateField);
        }
    }
}
