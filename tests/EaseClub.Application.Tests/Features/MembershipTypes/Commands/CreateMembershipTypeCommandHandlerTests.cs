using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Features.MembershipTypes.Commands.CreateMembershipType;
using EaseClub.Domain.Branches;
using EaseClub.Domain.MembershipTypes;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Tests.Features.MembershipTypes.Commands
{
    public class CreateMembershipTypeCommandHandlerTests
    {
        private readonly Mock<IMembershipTypeRepository> _membershipRepo = new();
        private readonly Mock<IBranchRepository> _branchRepo = new();
        private readonly Mock<IUnitOfWork> _uow = new();
        private readonly Mock<ILogger<CreateMembershipTypeCommandHandler>> _logger = new();

        private CreateMembershipTypeCommandHandler CreateHandler()
            => new(_membershipRepo.Object, _branchRepo.Object, _uow.Object, _logger.Object);

        [Fact]
        public async Task Handle_Should_Fail_When_Name_Is_Not_Unique()
        {
            var command = new CreateMembershipTypeCommand
            (
                ClubId: Guid.NewGuid(),
                Name: "Gold",
                Description: "Gold Membership",
                AllBranchesPermitted: true,
                BranchIds: new List<Guid>()
            );

            _membershipRepo
                .Setup(r => r.GetByClubIdAndNameAsync(command.ClubId, command.Name))
                .ReturnsAsync(MembershipType.Create(Guid.NewGuid(), command.ClubId, "Gold").Value);

            var handler = CreateHandler();

            var result = await handler.Handle(command, default);

            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(MembershipTypeErrors.MembershipTypeNameMustBeUniquePerClub);
        }

        [Fact]
        public async Task Handle_Should_Fail_When_BranchIds_Do_Not_Exist()
        {
            var branchId = Guid.NewGuid();

            var command = new CreateMembershipTypeCommand
            (
                ClubId: Guid.NewGuid(),
                Name: "Gold",
                Description: "Gold Membership",
                AllBranchesPermitted: false,
                BranchIds: new List<Guid> { branchId }
            );
            

            _membershipRepo
                .Setup(r => r.GetByClubIdAndNameAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync((MembershipType)null);

            _branchRepo
                .Setup(r => r.GetExistingBranchIdsAsync(It.IsAny<IEnumerable<Guid>>()))
                .ReturnsAsync(new HashSet<Guid>()); // none exist

            var handler = CreateHandler();

            var result = await handler.Handle(command, default);

            result.IsError.Should().BeTrue();
            result.TopError.Code.Should().Be("MembershipType.BranchDoesNotExist");
        }

        [Fact]
        public async Task Handle_Should_Create_MembershipType_Successfully()
        {
            var branchId = Guid.NewGuid();

            var command = new CreateMembershipTypeCommand
            (
                ClubId: Guid.NewGuid(),
                Name: "Gold",
                Description: "Premium membership",
                AllBranchesPermitted: true,
                BranchIds: new List<Guid>()
            );

            _membershipRepo
                .Setup(r => r.GetByClubIdAndNameAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync((MembershipType)null);

            _branchRepo
                .Setup(r => r.GetExistingBranchIdsAsync(It.IsAny<IEnumerable<Guid>>()))
                .ReturnsAsync(new HashSet<Guid> { branchId });

            var handler = CreateHandler();

            var result = await handler.Handle(command, default);

            result.IsSuccess.Should().BeTrue();

            _membershipRepo.Verify(r => r.AddAsync(It.IsAny<MembershipType>(), CancellationToken.None), Times.Once);
            _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
