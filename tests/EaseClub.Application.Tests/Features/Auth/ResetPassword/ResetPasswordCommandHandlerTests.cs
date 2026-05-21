using EaseClub.Application.Features.Auth.Commands.ResetPassword;
using EaseClub.Application.Features.Auth.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Common;

using Microsoft.Extensions.Logging;
using Moq;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;

namespace EaseClub.Application.Tests.Features.Auth.ResetPassword
{
    public class ResetPasswordCommandHandlerTests
    {
        private readonly Mock<IAuthIdentityService> _identityService = new();
        private readonly Mock<ILogger<ResetPasswordCommandHandler>> _logger = new();

        private ResetPasswordCommandHandler CreateHandler() =>
            new(_logger.Object, _identityService.Object);

        [Fact]
        public async Task Handle_ValidRequest_ReturnsSuccess()
        {
            var handler = CreateHandler();
            _identityService.Setup(s => s.ResetPasswordAsync("test@mail.com", "token", "NewPass123"))
                .ReturnsAsync(Result.Success);

            var result = await handler.Handle(new ResetPasswordCommand(new ResetPasswordDto
            {
                Email = "test@mail.com",
                Token = "token",
                NewPassword = "NewPass123"
            }), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_InvalidRequest_ReturnsError()
        {
            var handler = CreateHandler();
            _identityService.Setup(s => s.ResetPasswordAsync("test@mail.com", "token", "NewPass123"))
                .ReturnsAsync(Error.Failure("Failed"));

            var result = await handler.Handle(new ResetPasswordCommand(new ResetPasswordDto
            {
                Email = "test@mail.com",
                Token = "token",
                NewPassword = "NewPass123"
            }), CancellationToken.None);

            result.IsError.Should().BeTrue();
        }
    }
}
