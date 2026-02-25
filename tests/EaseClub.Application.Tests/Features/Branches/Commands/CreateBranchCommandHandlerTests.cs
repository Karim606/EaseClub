using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Features.Branches.Commands.CreateBranch;
using EaseClub.Domain.Branches;
using EaseClub.Domain.Clubs;
using EaseClub.Domain.Common;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Tests.Features.Branches.Commands
{
    public class CreateBranchCommandHandlerTests
    {
        private readonly Mock<IBranchRepository> _branchRepository = new();
        private readonly Mock<IClubRepository> _clubRepository = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly Mock<ILogger<CreateBranchCommandHandler>> _logger = new();

        private CreateBranchCommandHandler CreateHandler()
            => new(
                _branchRepository.Object,
                _clubRepository.Object,
                _unitOfWork.Object,
                _logger.Object
            );

        [Fact]
        public async Task Handle_Should_Return_NotFound_When_Club_Does_Not_Exist()
        {
            // Arrange
            var command = new CreateBranchCommand(Guid.NewGuid(), "Main Branch");

            _clubRepository
                .Setup(x => x.IsExistAsync(command.ClubId, CancellationToken.None))
                .ReturnsAsync(false);

            var handler = CreateHandler();

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.TopError.Type.Should().Be(ErrorKind.NotFound);

            _branchRepository.Verify(
                x => x.AddAsync(It.IsAny<Branch>(),CancellationToken.None),
                Times.Never
            );

            _unitOfWork.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never
            );
        }

        [Fact]
        public async Task Handle_Should_Return_Error_When_Branch_Creation_Fails()
        {
            // Arrange
            var command = new CreateBranchCommand(Guid.NewGuid(), " "); // invalid name

            _clubRepository
                .Setup(x => x.IsExistAsync(command.ClubId, CancellationToken.None))
                .ReturnsAsync(true);

            var handler = CreateHandler();

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(BranchErrors.NullOrWhiteSpaces);

            _branchRepository.Verify(
                x => x.AddAsync(It.IsAny<Branch>(), CancellationToken.None),
                Times.Never
            );

            _unitOfWork.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never
            );
        }

        [Fact]
        public async Task Handle_Should_Create_Branch_And_Save_When_Request_Is_Valid()
        {
            // Arrange
            var command = new CreateBranchCommand(Guid.NewGuid(), "Main Branch");

            _clubRepository
                .Setup(x => x.IsExistAsync(command.ClubId, CancellationToken.None))
                .ReturnsAsync(true);

            var handler = CreateHandler();

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBe(Guid.Empty);

            _branchRepository.Verify(
                x => x.AddAsync(It.Is<Branch>(b =>
                    b.ClubId == command.ClubId &&
                    b.Name == command.Name &&
                    b.IsActive
                ), CancellationToken.None),
                Times.Once
            );

            _unitOfWork.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Fact]
        public async Task Handle_Should_Return_Conflict_When_Branch_Name_Already_Exists()
        {
            // Arrange
            var command = new CreateBranchCommand(Guid.NewGuid(), "Main Branch");

            _clubRepository
                .Setup(x => x.IsExistAsync(command.ClubId, CancellationToken.None))
                .ReturnsAsync(true);

            _branchRepository
                .Setup(x => x.IsExistByName(command.ClubId, command.Name))
                .ReturnsAsync(true); // branch already exists

            var handler = CreateHandler();

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.TopError.Type.Should().Be(ErrorKind.Conflict);
            result.TopError.Description.Should().Contain(command.Name);

            _branchRepository.Verify(
                x => x.AddAsync(It.IsAny<Branch>(), CancellationToken.None),
                Times.Never
            );

            _unitOfWork.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never
            );
        }

    }
}
