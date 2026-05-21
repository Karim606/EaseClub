using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Features.Auth.Commands.Register;
using EaseClub.Application.Features.Auth.Common.Dtos;
using EaseClub.Application.Features.Auth.Common.Interfaces;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Member;

using FluentAssertions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Tests.Features.Auth.Register
{
    public class RegisterCommandHandlerTests
    {
        private readonly Mock<IAuthIdentityService> _identityService = new();
        private readonly Mock<IMemberUserRepository> _memberRepo = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly Mock<IAuthSessionService> _sessionService = new();
        private readonly Mock<ICurrentRequestContext> _context = new();
        private readonly Mock<ILogger<RegisterCommandHandler>> _logger = new();

        private RegisterCommandHandler CreateHandler() =>
            new(_identityService.Object, _memberRepo.Object, _unitOfWork.Object,
                _sessionService.Object, _context.Object, _logger.Object);

        [Fact]
        public async Task Handle_IdentityFails_ReturnsError()
        {
            //Arrange
            var handler = CreateHandler();
            _identityService.Setup(s => s.RegisterUserAsync("test@mail.com", "pass"))
                .ReturnsAsync(Error.Failure("Identity failed"));
            //Act
            var result = await handler.Handle(new RegisterCommand("John", "Doe", "test@mail.com", "pass", "0123456789"), CancellationToken.None);
            
            //Assert
            result.IsError.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_SuccessfulRegistration_ReturnsTokens()
        {
            //Arrange
            var handler = CreateHandler();
            var userId = Guid.NewGuid();

            _identityService.Setup(s => s.RegisterUserAsync("test@mail.com", "Pass123456"))
                .ReturnsAsync((Result<Guid>)userId);

            _sessionService.Setup(s => s.GenerateAuthTokens(userId, "John Doe", "test@mail.com", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new AuthTokensDto("access", "refresh", DateTime.UtcNow.AddHours(1)));

            _unitOfWork.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<IDbContextTransaction>());

            _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
            //Act

            var result = await handler.Handle(new RegisterCommand("John", "Doe", "test@mail.com", "Pass123456", "01012345678"), CancellationToken.None);

            //Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.AccessToken.Should().Be("access");
        }
    }
}
