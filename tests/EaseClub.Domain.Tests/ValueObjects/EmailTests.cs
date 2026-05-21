using EaseClub.Domain.Common.ValueObjects;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Tests.ValueObjects
{
    public class EmailTests
    {
        [Fact]
        public void Create_ShouldReturnSuccess_WhenEmailIsValid()
        {
            // Arrange
            var input = "Test@Example.com";

            // Act
            var result = Email.Create(input);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Value.Should().Be("test@example.com");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_ShouldFail_WhenEmailIsEmpty(string input)
        {
            // Act
            var result = Email.Create(input);

            // Assert
            result.IsError.Should().BeTrue();
            result.TopError.Description.Should().Be("Email cannot be empty.");
        }

        [Theory]
        [InlineData("test")]
        [InlineData("test@")]
        [InlineData("test@com")]
        [InlineData("test.com")]
        public void Create_ShouldFail_WhenEmailFormatIsInvalid(string input)
        {
            // Act
            var result = Email.Create(input);

            // Assert
            result.IsError.Should().BeTrue();
            result.TopError.Description.Should().Be("Invalid email format.");
        }
    }
}
