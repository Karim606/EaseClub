using EaseClub.Domain.Common;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Tests.Common
{
    public  class ErrorTests
    {
        [Fact]
        public void NotFound_ShouldCreateNotFoundError_WithCorrectDefaults()
        {
            // Act
            var error = Error.NotFound();

            // Assert
            error.Code.Should().Be(nameof(Error.NotFound));
            error.Description.Should().Be("Not found error");
            error.Type.Should().Be(ErrorKind.NotFound);
        }

        [Fact]
        public void Validation_ShouldCreateValidationError_WithCorrectDefaults()
        {
            //Arrange & Act
            var error = Error.Validation();

            //Assert
            error.Code.Should().Be(nameof(Error.Validation));
            error.Description.Should().Be("Validation error");
            error.Type.Should().Be(ErrorKind.Validation);
        }

        [Fact]
        public void Conflict_ShouldCreateConflictError_WithCorrectDefaults()
        {
            //Arrange & Act
            var error = Error.Conflict();

            //Assert
            error.Type.Should().Be(ErrorKind.Conflict);
        }

        [Fact]
        public void Create_ShouldCreateError_WithProvidedType()
        {
            // Act
            var error = Error.Create("CODE", "desc", (int)ErrorKind.Forbidden);

            // Assert
            error.Code.Should().Be("CODE");
            error.Description.Should().Be("desc");
            error.Type.Should().Be(ErrorKind.Forbidden);
        }

        [Fact]
        public void ToLogObject_ShouldReturnSerializableObject()
        {
            // Arrange
            var error = Error.Unauthorized();

            // Act
            var logObject = error.ToLogObject();

            // Assert
            logObject.Should().BeEquivalentTo(new
            {
                error.Code,
                error.Description,
                Type = error.Type.ToString()
            });
        }
    }
}
