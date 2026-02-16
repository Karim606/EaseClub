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
    public class ApplicationFieldValueTests
    {
        private readonly Guid _appId = Guid.NewGuid();
        private readonly Guid _sectionId = Guid.NewGuid();
        private readonly Guid _fieldId = Guid.NewGuid();

        [Fact]
        public void Create_ShouldReturnSuccess_WhenIdsAreValid()
        {
            // Act
            var result = ApplicationFieldValue.Create(Guid.NewGuid(), _appId, _sectionId, _fieldId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.ApplicationId.Should().Be(_appId);
        }

        [Fact]
        public void Create_ShouldFail_WhenApplicationIdIsEmpty()
        {
            // Act
            var result = ApplicationFieldValue.Create(Guid.NewGuid(), Guid.Empty, _sectionId, _fieldId);

            // Assert
            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(ApplicationFieldValueErrors.ApplicationRequired);
        }

        [Fact]
        public void SetNumber_ShouldClearPreviousStringValue()
        {
            // Arrange
            var fieldValue = ApplicationFieldValue.Create(Guid.NewGuid(), _appId, _sectionId, _fieldId).Value;
            fieldValue.SetString("Initial Text");

            // Act
            fieldValue.SetNumber(100.50m);

            // Assert
            fieldValue.NumberValue.Should().Be(100.50m);
            fieldValue.StringValue.Should().BeNull(); // Verification of ClearAll() logic
        }

        [Fact]
        public void SetString_ShouldFail_WhenValueIsWhiteSpace()
        {
            // Arrange
            var fieldValue = ApplicationFieldValue.Create(Guid.NewGuid(), _appId, _sectionId, _fieldId).Value;

            // Act
            var result = fieldValue.SetString("   ");

            // Assert
            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(ApplicationFieldValueErrors.InvalidStringValue);
        }

        [Fact]
        public void SetDate_ShouldOverwriteComplexJson()
        {
            // Arrange
            var fieldValue = ApplicationFieldValue.Create(Guid.NewGuid(), _appId, _sectionId, _fieldId).Value;
            fieldValue.SetComplexJson("{\"key\": \"value\"}");
            var now = DateTime.UtcNow;

            // Act
            fieldValue.SetDate(now);

            // Assert
            fieldValue.DateValue.Should().Be(now);
            fieldValue.ComplexJson.Should().BeNull();
        }
    }
}
