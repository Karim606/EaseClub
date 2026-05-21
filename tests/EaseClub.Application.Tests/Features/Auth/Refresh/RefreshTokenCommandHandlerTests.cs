using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Features.Auth.Commands.RefreshToken;
using EaseClub.Application.Features.Auth.Common.Dtos;
using EaseClub.Application.Features.Auth.Common.Interfaces;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Tests.Features.Auth.Refresh
{
    public class RefreshTokenCommandHandlerTests
    {
        private readonly Mock<IAuthSessionService> _sessionService = new();
        private readonly Mock<ICurrentRequestContext> _context = new();
        private readonly Mock<ILogger<RefreshTokenCommandHandler>> _logger = new();

        private RefreshTokenCommandHandler CreateHandler() =>
            new(_sessionService.Object, _context.Object, _logger.Object);

        [Fact]
        public async Task Handle_ValidToken_ReturnsTokens()
        {
            //Arrange
            var handler = CreateHandler();

            _sessionService.Setup(s => s.RefreshAsync("token", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((Result<AuthTokensDto>)new AuthTokensDto("access", "refresh", DateTime.UtcNow.AddHours(1)));

            //Act
            var result = await handler.Handle(new RefreshTokenCommand("token"), CancellationToken.None);

            //Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.AccessToken.Should().Be("access");
        }

        [Fact]
        public async Task Handle_InvalidToken_ReturnsError()
        {
            //Arrange
            var handler = CreateHandler();

            _sessionService.Setup(s => s.RefreshAsync("bad", It.IsAny<string>(),It.IsAny<string>() ))
                .ReturnsAsync((Result<AuthTokensDto>)Error.Failure("Invalid"));

            //Act
            var result = await handler.Handle(new RefreshTokenCommand("bad"), CancellationToken.None);

            //Assert
            result.IsError.Should().BeTrue();
        }
    }
}
