using EaseClub.Application.Features.MembershipTypes.Queries.GetMembershipTypesByClub;
using EaseClub.Domain.MembershipTypes;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Tests.Features.MembershipTypes.Queries
{
    public class GetMembershipTypesByClubQueryHandlerTests
    {
        [Fact]
        public async Task Handle_Should_Return_Mapped_Dtos()
        {
            var clubId = Guid.NewGuid();

            var type = MembershipType.Create(Guid.NewGuid(), clubId, "Gold").Value;

            var repo = new Mock<IMembershipTypeRepository>();
            repo.Setup(r => r.GetByClubIdAsync(clubId))
                .ReturnsAsync(new List<MembershipType> { type });

            var handler = new GetMembershipTypesByClubQueryHandler(repo.Object);

            var result = await handler.Handle(new GetMembershipTypesByClubQuery(clubId), default);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(1);
            result.Value[0].Name.Should().Be("Gold");
        }
    }
}
