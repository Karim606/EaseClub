using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Features.MembershipTypes.Commands.ToggleMembershipTypeActivation;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipTypes;
using FluentAssertions;
using Moq;

namespace EaseClub.Application.Tests.Features.MembershipTypes.Commands
{
    public class ToggleTypeActivationHandlerTests
    {
        private readonly Mock<IMembershipTypeRepository> _membershipRepo = new();
        private readonly Mock<IUnitOfWork> _uow = new();

        private ToggleTypeActivationHandler CreateHandler()
            => new(_membershipRepo.Object, _uow.Object);

        [Fact]
        public async Task Handle_Should_Return_NotFound_When_MembershipType_Not_Found()
        {
            var id = Guid.NewGuid();

            _membershipRepo
                .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((MembershipType)null);

            var handler = CreateHandler();

            var command = new ToggleTypeActivationCommand(Guid.NewGuid(), id);

            var result = await handler.Handle(command, default);

            result.IsError.Should().BeTrue();
            result.TopError.Type.Should().Be(ErrorKind.NotFound);
        }

        [Fact]
        public async Task Handle_Should_Deactivate_When_Type_Is_Active()
        {
            var clubId = Guid.NewGuid();
            var id = Guid.NewGuid();

            var type = MembershipType.Create(id, clubId, "Gold").Value;

            _membershipRepo
                .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(type);

            var handler = CreateHandler();

            var command = new ToggleTypeActivationCommand(clubId, id);

            var result = await handler.Handle(command, default);

            result.IsSuccess.Should().BeTrue();
            type.IsActive.Should().BeFalse();

            _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_Activate_When_Type_Is_Inactive()
        {
            var clubId = Guid.NewGuid();
            var id = Guid.NewGuid();

            var type = MembershipType.Create(id, clubId, "Gold").Value;
            type.Deactivate(); // make it inactive

            _membershipRepo
                .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(type);

            var handler = CreateHandler();

            var command = new ToggleTypeActivationCommand(clubId, id);

            var result = await handler.Handle(command, default);

            result.IsSuccess.Should().BeTrue();
            type.IsActive.Should().BeTrue();

            _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
