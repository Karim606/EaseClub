using EaseClub.Application.Features.MembershipPlans.Queries.GetMembershipPlansByClub;
using EaseClub.Domain.MembershipPlans;
using EaseClub.Domain.MembershipPlans.Repositories;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Tests.Features.MembershipPlans.Queries
{
    public class GetMembershipPlansByClubHandlerTests
    {
        private readonly Mock<IMembershipPlanRepository> _planRepo = new();
        private readonly GetMembershipPlansByClubHandler _handler;

        public GetMembershipPlansByClubHandlerTests()
        {
            _handler = new GetMembershipPlansByClubHandler(_planRepo.Object);
        }

        [Fact]
        public async Task Handle_Should_Return_Empty_List_When_No_Plans_Exist()
        {
            var clubId = Guid.NewGuid();

            _planRepo
                .Setup(x => x.GetPlansByClubAsync(clubId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<MembershipPlan>());

            var result = await _handler.Handle(
                new GetMembershipPlansByClubQuery(clubId),
                CancellationToken.None);

            result.IsError.Should().BeFalse();
            result.Value.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_Should_Return_Mapped_Plans()
        {
            var clubId = Guid.NewGuid();

            var plans = new List<MembershipPlan>
            {
                MembershipPlan.Create(Guid.NewGuid(), clubId, Guid.NewGuid(), "Gold", 2000, 60).Value,
                MembershipPlan.Create(Guid.NewGuid(), clubId, Guid.NewGuid(), "Silver", 1500, 30).Value
            };

            _planRepo
                .Setup(x => x.GetPlansByClubAsync(clubId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(plans);

            var result = await _handler.Handle(
                new GetMembershipPlansByClubQuery(clubId),
                CancellationToken.None);

            result.IsError.Should().BeFalse();
            result.Value.Should().HaveCount(2);
            result.Value.Select(x => x.Name).Should().Contain(new[] { "Gold", "Silver" });
        }
    }
}
