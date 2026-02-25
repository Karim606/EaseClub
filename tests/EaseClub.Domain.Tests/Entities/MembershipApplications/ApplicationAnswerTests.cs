using EaseClub.Domain.MembershipApplications;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Tests.Entities.MembershipApplications
{
    public class ApplicationAnswerTests
    {
        [Fact]
        public void SetAnswer_ShouldAddNewAnswer_WhenNotExists()
        {
            // Arrange
            var app = CreateValidApplication();
            var fieldId = Guid.NewGuid();

            // Act
            var result = app.SetAnswer("FirstName", fieldId, "John", instanceIndex: 0);

            // Assert
            result.IsSuccess.Should().BeTrue();
            app.Answers.Should().HaveCount(1);
            app.Answers.First().Value.Should().Be("John");
        }

        [Fact]
        public void SetAnswer_ShouldUpdateExisting_WhenKeyAndIndexMatch()
        {
            // Arrange
            var app = CreateValidApplication();
            var fieldId = Guid.NewGuid();
            app.SetAnswer("FirstName", fieldId, "John", 0);

            // Act
            app.SetAnswer("FirstName", fieldId, "Jane", 0);

            // Assert
            app.Answers.Should().HaveCount(1);
            app.Answers.First().Value.Should().Be("Jane");
        }

        private MembershipApplication CreateValidApplication() =>
            MembershipApplication.Create(Guid.NewGuid(), "TRK", "{}", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 0).Value;
    }
}
