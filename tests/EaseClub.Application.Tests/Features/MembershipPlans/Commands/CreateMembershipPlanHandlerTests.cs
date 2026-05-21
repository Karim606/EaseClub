using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Features.MembershipPlans.Command.CreatePlan;
using EaseClub.Domain.Common;
using EaseClub.Domain.MembershipPlans;
using EaseClub.Domain.MembershipPlans.Repositories;
using EaseClub.Domain.MembershipTypes;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Tests.Features.MembershipPlans.Commands
{
    public class CreateMembershipPlanHandlerTests
    {
        private readonly Mock<IMembershipPlanRepository> _planRepo = new();
        private readonly Mock<IMembershipTypeRepository> _membershipTypeRepo = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly Mock<ILogger<CreateMembershipPlanHandler>> _logger = new();

        private readonly CreateMembershipPlanHandler _handler;

        public CreateMembershipPlanHandlerTests()
        {
            _handler = new CreateMembershipPlanHandler(
                _planRepo.Object,
                _unitOfWork.Object,
                _membershipTypeRepo.Object,
                _logger.Object);
        }

        [Fact]
        public async Task Handle_Should_Return_NotFound_When_MembershipType_Does_Not_Exist()
        {
            var command = new CreateMembershipPlanCommand(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Gold",
                2000,
                60,1,2);

            _membershipTypeRepo
                .Setup(x => x.GetByIdAsync(command.MembershipTypeId, CancellationToken.None))
                .ReturnsAsync((MembershipType?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsError.Should().BeTrue();
            result.TopError.Type.Should().Be(ErrorKind.NotFound);
        }

        [Fact]
        public async Task Handle_Should_Return_Error_When_Name_Already_Exists_In_Club()
        {
            var membershipType = MembershipType.Create(Guid.NewGuid(),Guid.NewGuid(), "Type").Value;

            var command = new CreateMembershipPlanCommand(
                membershipType.ClubId,
                membershipType.Id,
                "Gold",
                2000,
                60,1,2);

            _membershipTypeRepo
                .Setup(x => x.GetByIdAsync(command.MembershipTypeId, CancellationToken.None))
                .ReturnsAsync(membershipType);

            _planRepo
                .Setup(x => x.ExistsByNameAsync(command.ClubId, command.Name, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(MembershipPlanErrors.NameAlreadyExistsInClub);
        }

        [Fact]
        public async Task Handle_Should_Save_And_Return_Id_When_Creation_Is_Successful()
        {
            var membershipType = MembershipType.Create(Guid.NewGuid(),Guid.NewGuid(), "Type").Value;

            var command = new CreateMembershipPlanCommand(
                membershipType.ClubId,
                membershipType.Id,
                "Gold",
                2000,
                60,1,2);

            _membershipTypeRepo
                .Setup(x => x.GetByIdAsync(command.MembershipTypeId, CancellationToken.None))
                .ReturnsAsync(membershipType);

            _planRepo
                .Setup(x => x.ExistsByNameAsync(command.ClubId, command.Name, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsError.Should().BeFalse();
            result.Value.Should().NotBe(Guid.Empty);

            _planRepo.Verify(x => x.AddAsync(It.IsAny<MembershipPlan>(), CancellationToken.None), Times.Once);
            _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
