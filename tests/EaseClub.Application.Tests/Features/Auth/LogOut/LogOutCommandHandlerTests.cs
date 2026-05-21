using EaseClub.Application.Features.Auth.Commands.LogOut;
using EaseClub.Application.Features.Auth.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Common;


using FluentAssertions;
using Microsoft.Extensions.Logging;

using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Tests.Features.Auth.LogOut
{
    public class LogOutCommandHandlerTests
    {
        private readonly Mock<IAuthSessionService> _sessionService = new();
        private readonly Mock<ILogger<LogOutCommandHandler>> _logger = new();

        private LogOutCommandHandler CreateHandler() =>
            new(_sessionService.Object, _logger.Object);

        [Fact]
        public async Task Handle_ValidToken_ReturnsSuccess()
        {
            //Arrange
            var handler = CreateHandler();
            _sessionService.Setup(s => s.LogoutAsync("token"))
                .ReturnsAsync(Result.Success);
            //Act
            var result = await handler.Handle(new LogOutCommand("token"), CancellationToken.None);

            //Assert
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_InvalidToken_ReturnsError()
        {
            //Arrange
            var handler = CreateHandler();
            _sessionService.Setup(s => s.LogoutAsync("bad"))
                .ReturnsAsync(Error.Failure(description:"Invalid token"));

            //Act
            var result = await handler.Handle(new LogOutCommand("bad"), CancellationToken.None);

            //Assert
            result.IsSuccess.Should().BeFalse();
        }

    }
}
