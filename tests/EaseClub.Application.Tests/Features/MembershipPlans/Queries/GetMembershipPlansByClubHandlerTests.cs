using EaseClub.Application.Common.Pagination.Parameters;
using EaseClub.Application.Common.Pagination.Results;
using EaseClub.Application.Features.MembershipPlans.Queries;
using EaseClub.Application.Features.MembershipPlans.Queries.GetMembershipPlansByClub;
using EaseClub.Domain.Common.Results;
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
        private readonly Mock<IMembershipPlanQueryService> _queryServiceMock = new();
        private readonly GetMembershipPlansByClubHandler _handler;

        public GetMembershipPlansByClubHandlerTests()
        {
            _handler = new GetMembershipPlansByClubHandler(_queryServiceMock.Object);
        }

        [Fact]
        public async Task Handle_Should_Return_Empty_Paginated_Result_When_No_Plans_Exist()
        {
            // Arrange
            var clubId = Guid.NewGuid();
            var query = new GetMembershipPlansByClubQuery(clubId, new OffsetPaginationParameters());

            // Create a valid Result containing an empty paginated object
            var emptyPaginatedResult = new OffsetPaginatedResult<MembershipPlanDto>
            {
                Items = new List<MembershipPlanDto>(),
                TotalCount = 0,
                Page = 1,
                TotalPages = 0
            };
            var serviceResult = emptyPaginatedResult;

            _queryServiceMock
                .Setup(x => x.GetMembershipPlansByClubAsync<OffsetPaginatedResult<MembershipPlanDto>>(
                    clubId,
                    query.Parameters,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsError.Should().BeFalse();
            result.Value.Items.Should().BeEmpty();
            result.Value.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_Should_Return_Mapped_Plans_When_Data_Exists()
        {
            // Arrange
            var clubId = Guid.NewGuid();
            var query = new GetMembershipPlansByClubQuery(clubId, new OffsetPaginationParameters());

            var dtos = new List<MembershipPlanDto>
            {
                new MembershipPlanDto(Guid.NewGuid(), "Gold", 60, 2000, "Premium Plan"),
                new MembershipPlanDto(Guid.NewGuid(), "Silver", 30, 1500, "Standard Plan")
            };

            var paginatedResult = new OffsetPaginatedResult<MembershipPlanDto>
            {
                Items = dtos,
                TotalCount = 2,
                Page = 1,
                TotalPages = 1
            };

            _queryServiceMock
                .Setup(x => x.GetMembershipPlansByClubAsync<OffsetPaginatedResult<MembershipPlanDto>>(
                    clubId,
                    query.Parameters,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(paginatedResult);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsError.Should().BeFalse();
            result.Value.Items.Should().HaveCount(2);
            result.Value.Items.Should().Contain(x => x.Name == "Gold");
            result.Value.Items.Should().Contain(x => x.Name == "Silver");
        }
    }
}
