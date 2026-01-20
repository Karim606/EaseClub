using EaseClub.Application.Common.Behaviors;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using EaseClub.Application.Tests.Common.Behaviors.sharedSetupForTests;

using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Tests.Common.Behaviors
{
    public class LoggingBehaviorTests
    {
        [Fact]
        public async Task Should_Log_Success_When_Response_Is_Success()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LoggingBehavior<TestRequest, Result<TestResponse>>>>();
            var behavior = new LoggingBehavior<TestRequest, Result<TestResponse>>(loggerMock.Object);

            var request = new TestRequest();
            var response = new TestResponse { Data = "Test Data" };

            // Act
            var result = await behavior.Handle(request, (CancellationToken) => Task.FromResult((Result<TestResponse>)response), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(response);
            loggerMock.VerifyLog(LogLevel.Information, "processing Request", Times.Once());
            loggerMock.VerifyLog(LogLevel.Information, "processed successfully", Times.Once());

        }

        [Fact]
        public async Task Should_Log_Warning_When_Response_Is_Failure()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LoggingBehavior<TestRequest, Result<TestResponse>>>>();
            var behavior = new LoggingBehavior<TestRequest,Result<TestResponse>>(loggerMock.Object);

            var request = new TestRequest();
            var response = (Result<TestResponse>)Error.Failure(description:"general failure");

            // Act
            var result = await behavior.Handle(request, (CancellationToken) => Task.FromResult(response), CancellationToken.None);

            //Assert
            result.Should().BeSameAs(response);
            loggerMock.VerifyLog(LogLevel.Warning, "processed with errors", Times.Once());
        }

    }

    public static class LoggerMockExtensions
    {
        public static void VerifyLog<T>(this Mock<ILogger<T>> loggerMock, LogLevel level, string messagePart, Times times)
        {
            loggerMock.Verify(
                x => x.Log(
                    level,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString().Contains(messagePart)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                times);
        }
    }
}
