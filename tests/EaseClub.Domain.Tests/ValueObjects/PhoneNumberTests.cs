using EaseClub.Domain.Common.ValueObjects;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Tests.ValueObjects
{
    public class PhoneNumberTests
    {
        [Fact]
        public void Create_ShouldReturnSuccess_WhenPhoneNumberIsValid()
        {
            // Arrange
            var input = "01012345678";

            // Act
            var result = PhoneNumber.Create(input);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Value.Should().Be("01012345678");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_ShouldFail_WhenPhoneNumberIsEmpty(string input)
        {
            // Act
            var result = PhoneNumber.Create(input);

            // Assert
            result.IsError.Should().BeTrue();
            result.TopError.Description.Should().Be("Phone number cannot be empty.");
        }

        [Theory]
        [InlineData("012345678")]
        [InlineData("02012345678")]
        [InlineData("011123")]
        [InlineData("abc")]
        public void Create_ShouldFail_WhenPhoneNumberFormatIsInvalid(string input)
        {
            // Act
            var result = PhoneNumber.Create(input);

            // Assert
            result.IsError.Should().BeTrue();
            result.TopError.Description.Should().Be("Invalid Egyptian phone number format.");
        }
    }
}
