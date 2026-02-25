using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Features.MembershipTypes.Commands.DeactivateMembershipType;
using EaseClub.Domain.ClubAdmin;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.ValueObjects;
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
    public class DeactivateMembershipTypeCommandHandlerTests
    {
        private readonly Mock<IMembershipTypeRepository> _membershipRepo = new();
        private readonly Mock<IClubAdminUserRepository> _clubAdminRepo = new();
        private readonly Mock<ICurrentUserService> _currentUser = new();
        private readonly Mock<IUnitOfWork> _uow = new();
        private readonly Mock<ILogger<DeactivateMembershipTypeCommandHandler>> _logger = new();

        private DeactivateMembershipTypeCommandHandler CreateHandler()
            => new(_membershipRepo.Object, _logger.Object, _uow.Object,
                   _currentUser.Object, _clubAdminRepo.Object);

        [Fact]
        public async Task Handle_Should_Return_Unauthorized_When_User_Not_Found()
        {
            _currentUser.Setup(c => c.GetId()).Returns(Guid.NewGuid().ToString());
            _clubAdminRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), CancellationToken.None))
                .ReturnsAsync((ClubAdminUser)null);

            var handler = CreateHandler();

            var result = await handler.Handle(new DeactivateMembershipTypeCommand(Guid.NewGuid()), default);

            result.IsError.Should().BeTrue();
            result.TopError.Type.Should().Be(ErrorKind.Unauthorized);
        }

        [Fact]
        public async Task Handle_Should_Deactivate_When_Valid()
        {
            var clubId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            
            var phoneNumber = PhoneNumber.Create("01112805432").Value;
            var email = Email.Create("adm@example.com").Value;

            var type = MembershipType.Create(Guid.NewGuid(), clubId, "Gold").Value;

            _currentUser.Setup(c => c.GetId()).Returns(adminId.ToString());
            _clubAdminRepo.Setup(r => r.GetByIdAsync(adminId, CancellationToken.None))
                .ReturnsAsync(ClubAdminUser.Create(adminId, clubId, "Ali", "Ali", phoneNumber,email));
            _membershipRepo.Setup(r => r.GetByIdAsync(type.Id,CancellationToken.None)).ReturnsAsync(type);

            var handler = CreateHandler();

            var result = await handler.Handle(new DeactivateMembershipTypeCommand(type.Id), default);

            result.IsSuccess.Should().BeTrue();
            type.IsActive.Should().BeFalse();

            _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
