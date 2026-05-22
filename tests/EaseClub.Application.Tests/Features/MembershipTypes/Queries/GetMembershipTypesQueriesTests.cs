using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Common.Pagination;
using EaseClub.Application.Features.MembershipTypes;
using EaseClub.Application.Features.MembershipTypes.Queries;
using EaseClub.Application.Features.MembershipTypes.Queries.GetMembershipTypesForAdmin;
using EaseClub.Application.Features.MembershipTypes.Queries.GetMembershipsTypesForMember;
using EaseClub.Domain.Common.Results;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EaseClub.Application.Tests.Features.MembershipTypes.Queries
{
    public class GetMembershipTypesQueriesTests
    {
        private readonly Mock<IMembershipTypesQueryService> _queryServiceMock = new();
        private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
        private readonly Mock<ILogger<GetMembershipTypesForAdminQueryHandler>> _adminLoggerMock = new();
        private readonly Mock<ILogger<GetMembershipTypesForMemberQueryHandler>> _memberLoggerMock = new();

        [Fact]
        public async Task GetMembershipTypesForAdminQueryHandler_Should_Call_QueryService_And_Return_Result()
        {
            // Arrange
            var clubId = Guid.NewGuid();
            var branchId = Guid.NewGuid();
            var pagination = new PaginationRequest { Page = 1, Limit = 10 };
            var query = new GetMembershipTypesForAdminQuery(clubId, branchId, true, true, pagination);

            var items = new List<MembershipTypeAdminDto>
            {
                new(Guid.NewGuid(), "Admin Type", "Desc", true, true, DateTime.UtcNow)
            };
            var expectedResponse = new UnifiedPaginatedResponse<MembershipTypeAdminDto>(items, false, Page: 1, TotalCount: 1);

            _queryServiceMock
                .Setup(x => x.GetMembershipTypesForAdminAsync(clubId, branchId, true, true, pagination, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var handler = new GetMembershipTypesForAdminQueryHandler(_queryServiceMock.Object, _adminLoggerMock.Object);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(expectedResponse);
            _queryServiceMock.Verify(x => x.GetMembershipTypesForAdminAsync(clubId, branchId, true, true, pagination, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetMembershipTypesForMemberQueryHandler_Should_Call_QueryService_And_Return_Result()
        {
            // Arrange
            var clubId = Guid.NewGuid();
            var branchId = Guid.NewGuid();
            var pagination = new PaginationRequest { Page = 1, Limit = 10 };
            var query = new GetMembershipTypesForMemberQuery(clubId, branchId, true, pagination);

            var items = new List<MembershipTypeDto>
            {
                new(Guid.NewGuid(), "Member Type", "Desc", true)
            };
            var expectedResponse = new UnifiedPaginatedResponse<MembershipTypeDto>(items, false, Page: 1, TotalCount: 1);

            _queryServiceMock
                .Setup(x => x.GetMembershipTypesForUserAsync(clubId, branchId, true, true, pagination, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var handler = new GetMembershipTypesForMemberQueryHandler(
                _currentUserServiceMock.Object,
                _queryServiceMock.Object,
                _memberLoggerMock.Object);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(expectedResponse);
            _queryServiceMock.Verify(x => x.GetMembershipTypesForUserAsync(clubId, branchId, true, true, pagination, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
