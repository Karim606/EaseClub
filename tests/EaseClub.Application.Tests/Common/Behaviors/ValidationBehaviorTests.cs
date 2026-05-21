using EaseClub.Application.Common.Behaviors;
using EaseClub.Application.Tests.Common.Behaviors.sharedSetupForTests;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Tests.Common.Behaviors
{
    public class ValidationBehaviorTests
    {
        [Fact]
        public async Task Should_Call_Next_When_Validator_Is_Null()
        {
            // Arrange
            var behavior = new ValidationBehavior<TestRequest, Result<TestResponse>>(null);
            var called = false;

            // Act
            var result = await behavior.Handle(new TestRequest(), (CancellationToken) =>
            {
                called = true;
                return Task.FromResult((Result<TestResponse>)new TestResponse());
            }, CancellationToken.None);

            // Assert
            called.Should().BeTrue();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Should_Return_ValidationErrors_When_Validation_Fails()
        {
            // Arrange
            var validatorMock = new Mock<IValidator<TestRequest>>();
            validatorMock.Setup(v => v.ValidateAsync(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(new[]
                {
                    new ValidationFailure("Email", "Invalid email")
                }));

            var behavior = new ValidationBehavior<TestRequest, Result<TestResponse>>(validatorMock.Object);
            var request = new TestRequest();

            // Act
            var result = await behavior.Handle(request, (CancellationToken) => Task.FromResult((Result<TestResponse>)(new TestResponse())),
                CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.Code == "Email" && e.Description == "Invalid email");
        }

        [Fact]
        public async Task Should_Call_Next_When_Validation_Passes()
        {
            // Arrange
            var validatorMock = new Mock<IValidator<TestRequest>>();
            validatorMock.Setup(v => v.ValidateAsync(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            var behavior = new ValidationBehavior<TestRequest, Result<TestResponse>>(validatorMock.Object);
            var called = false;

            // Act
            var result = await behavior.Handle(new TestRequest(), (CancellationToken) =>
            {
                called = true;
                return Task.FromResult((Result<TestResponse>)new TestResponse());
            }, CancellationToken.None);

            // Assert
            called.Should().BeTrue();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Should_Filter_Null_Errors()
        {
            //Arrange
            var validatorMock = new Mock<IValidator<TestRequest>>();
            validatorMock.Setup(v => v.ValidateAsync(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(new ValidationFailure?[] { null }));
            var behavior = new ValidationBehavior<TestRequest, Result<TestResponse>>(validatorMock.Object);

            //Act
            var result = await behavior.Handle(new TestRequest(), (CancellationToken) => Task.FromResult((Result<TestResponse>)(new TestResponse())), 
                CancellationToken.None);

            //Assert
            result.IsSuccess.Should().BeTrue(); // no valid errors → treated as success
        }
    }
}
