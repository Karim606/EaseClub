using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Common.Pagination;
using EaseClub.Application.Features.MembershipPlans.Queries;
using EaseClub.Application.Features.MembershipPlans.Queries.GetMembershipPlansForAdmin;
using EaseClub.Application.Features.MembershipPlans.Queries.GetMembershipPlansForMember;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipPlans;
using EaseClub.Domain.MembershipPlans.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EaseClub.Application.Tests.Features.MembershipPlans.Queries
{
    public class GetMembershipPlansQueriesTests
    {
        private readonly Mock<IMembershipPlanQueryService> _queryServiceMock = new();
        private readonly Mock<IMembershipPlanRepository> _repositoryMock = new();
        private readonly Mock<IClubAuthorizationService> _authServiceMock = new();
        private readonly Mock<ILogger<GetMembershipPlansForAdminHandler>> _adminLoggerMock = new();
        private readonly Mock<ILogger<GetMembershipPlansForMemberQueryHandler>> _memberLoggerMock = new();

        [Fact]
        public async Task GetMembershipPlansForAdminHandler_Should_Call_QueryService_And_Return_Result()
        {
            // Arrange
            var clubId = Guid.NewGuid();
            var typeId = Guid.NewGuid();
            var pagination = new PaginationRequest { Page = 1, Limit = 10 };
            var query = new GetMembershipPlansForAdminQuery(clubId, typeId, true, pagination);

            var items = new List<MembershipPlanAdminDto>
            {
                new() { Id = Guid.NewGuid(), Name = "Plan A", IsActive = true }
            };
            var expectedResponse = new UnifiedPaginatedResponse<MembershipPlanAdminDto>(items, false, Page: 1, TotalCount: 1);
            
            _queryServiceMock
                .Setup(x => x.GetMembershipPlansForAdminAsync(clubId, typeId, true, pagination, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var handler = new GetMembershipPlansForAdminHandler(_queryServiceMock.Object, _adminLoggerMock.Object);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(expectedResponse);
            _queryServiceMock.Verify(x => x.GetMembershipPlansForAdminAsync(clubId, typeId, true, pagination, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetMembershipPlansForMemberQueryHandler_Should_Call_QueryService_And_Return_Result()
        {
            // Arrange
            var clubId = Guid.NewGuid();
            var typeId = Guid.NewGuid();
            var pagination = new PaginationRequest { Page = 1, Limit = 10 };
            var query = new GetMembershipPlansForMemberQuery(clubId, typeId, pagination);

            var items = new List<MembershipPlanDto>
            {
                new(Guid.NewGuid(), "Plan B", 30, 1, 0, 100, EnrollmentMode.ApplicationForm, PaymentMode.Cash, false, 80, "Desc")
            };
            var expectedResponse = new UnifiedPaginatedResponse<MembershipPlanDto>(items, false, Page: 1, TotalCount: 1);

            _queryServiceMock
                .Setup(x => x.GetMembershipPlansForMemberAsync(clubId, typeId, pagination, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var handler = new GetMembershipPlansForMemberQueryHandler(
                _repositoryMock.Object,
                _queryServiceMock.Object,
                _authServiceMock.Object,
                _memberLoggerMock.Object);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(expectedResponse);
            _queryServiceMock.Verify(x => x.GetMembershipPlansForMemberAsync(clubId, typeId, pagination, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
