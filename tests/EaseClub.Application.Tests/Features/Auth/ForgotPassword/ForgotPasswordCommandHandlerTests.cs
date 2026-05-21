using EaseClub.Application.Features.Auth.Commands.ForgotPassword;
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

namespace EaseClub.Application.Tests.Features.Auth.ForgotPassword
{
    public class ForgotPasswordCommandHandlerTests
    {
        private readonly Mock<IAuthIdentityService> _identityService = new();
        private readonly Mock<ILogger<ForgotPasswordCommand>> _logger = new();

        private ForgotPasswordHandler CreateHandler() =>
            new(_logger.Object, _identityService.Object);

        [Fact]
        public async Task Handle_Success_ReturnsSuccess()
        {
            var handler = CreateHandler();
            _identityService.Setup(s => s.RequestResetPasswordAsync("test@mail.com"))
                .ReturnsAsync(Result.Success);

            var result = await handler.Handle(new ForgotPasswordCommand("test@mail.com"), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_Failure_ReturnsFailure()
        {
            var handler = CreateHandler();
            _identityService.Setup(s => s.RequestResetPasswordAsync("test@mail.com"))
                .ReturnsAsync(Error.Failure("Failed"));

            var result = await handler.Handle(new ForgotPasswordCommand("test@mail.com"), CancellationToken.None);

            result.IsError.Should().BeTrue();
        }
    }
}
