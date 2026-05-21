using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Features.Auth.Commands.Login;
using EaseClub.Application.Features.Auth.Common.Dtos;
using EaseClub.Application.Features.Auth.Common.Interfaces;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.ValueObjects;
using EaseClub.Domain.Member;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Tests.Features.Auth.Login
{
    public class LoginCommandHandlerTests
    {
        private readonly Mock<IUserBaseRepository> _usersBaseRepo = new();
        private readonly Mock<IAuthSessionService> _sessionService = new();
        private readonly Mock<ICurrentRequestContext> _context = new();
        private readonly Mock<ILogger<LoginCommandHandler>> _logger = new();

        private LoginCommandHandler CreateHandler()
            => new(
                _usersBaseRepo.Object,
                _sessionService.Object,
                _context.Object,
                _logger.Object
            );

        [Fact]
        public async Task Handle_UserNotFound_ReturnsUnauthorized()
        {
            // Arrange
            _usersBaseRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
                       .ReturnsAsync((UserBase?)null);

            var handler = CreateHandler();
            var command = new LoginCommand("test@mail.com", "123456");

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.TopError.Type.Should().Be(Domain.Common.ErrorKind.Unauthorized);
        }

        [Fact]
        public async Task Handle_ValidCredentials_ReturnsTokens()
        {
            // Arrange
            var user = MemberUser.Create(
                Guid.NewGuid(),
                "John",
                "Doe",
                PhoneNumber.Create("01012345678").Value,
                Email.Create("test@mail.com").Value
            );

            _usersBaseRepo.Setup(r => r.GetByEmailAsync(user.Email.Value))
                       .ReturnsAsync(user);

            _sessionService.Setup(s => s.LoginAsync(
                    user.Email.Value,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(new AuthTokensDto("access", "refresh", DateTime.UtcNow));

            var handler = CreateHandler();
            var command = new LoginCommand(user.Email.Value, "password");

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.AccessToken.Should().Be("access");
        }
    }
}
