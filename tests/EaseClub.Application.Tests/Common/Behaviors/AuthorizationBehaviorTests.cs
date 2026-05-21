using EaseClub.Application.Common;
using EaseClub.Application.Common.Behaviors;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Tests.Common.Behaviors.sharedSetupForTests;
using EaseClub.Domain.ClubAdmin;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Common.ValueObjects;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Tests.Common.Behaviors
{
    public class AuthorizationBehaviorTests
    {
        private readonly Mock<IClubAuthorizationService> _clubAuthorizationService = new();
        private readonly Mock<ICurrentUserService> _currentUserService = new();
        private readonly Mock<IClubAdminUserRepository> _clubAdminUserRepository = new();

        #region Test Request Types

        public class TestResponse { }

        public class TestRequestRequireClubAdmin : IAuthorizeRequest, IRequireClubAdmin
        {
            public Guid ClubId { get; init; }
        }

        public class TestRequestRequireMembership : IAuthorizeRequest, IRequireMembership
        {
            public Guid ClubId { get; init; }
        }

        public class TestOwnershipRequest : IAuthorizeRequest, IRequireClubOwnershipValidation
        {
            public Guid ClubId { get; init; }

            public IEnumerable<OwnershipRule> Rules()
            {
                yield return new OwnershipRule(
                    (auth, clubId) => Task.FromResult(false), // force ownership failure
                    "Resource",
                    Guid.NewGuid());
            }
        }

        #endregion

        #region Helpers

        private AuthorizationBehavior<TRequest, Result<TestResponse>> CreateBehavior<TRequest>()
            where TRequest : notnull, IAuthorizeRequest
        {
            return new AuthorizationBehavior<TRequest, Result<TestResponse>>(
                _clubAuthorizationService.Object,
                _currentUserService.Object,
                _clubAdminUserRepository.Object,
                Mock.Of<ILogger<AuthorizationBehavior<TRequest, Result<TestResponse>>>>());
        }

        private Task<Result<TestResponse>> Next() =>
            Task.FromResult( (Result<TestResponse>)new TestResponse() );

        private (Guid userId, Guid clubId) SetupUser(IEnumerable<string>? roles = null, string? userIdValue = null)
        {
            var userId = Guid.NewGuid();
            var clubId = Guid.NewGuid();

            _currentUserService.Setup(c => c.GetId()).Returns(userIdValue ?? userId.ToString());
            _currentUserService.Setup(c => c.GetRoles()).Returns(roles is null ? new List<string>() : new List<string>(roles));

            return (userId, clubId);
        }

        #endregion

        #region Tests

        [Fact]
        public async Task Handle_UserIdEmpty_Returns_Unauthorized()
        {
            _currentUserService.Setup(c => c.GetId()).Returns(string.Empty);
            var behavior = CreateBehavior<TestRequestRequireClubAdmin>();

            var result = await behavior.Handle(
                new TestRequestRequireClubAdmin { ClubId = Guid.NewGuid() },
                _ => Next(),
                CancellationToken.None);

            result.IsError.Should().BeTrue();
            result.TopError.Type.Should().Be(ErrorKind.Unauthorized);
        }

        [Fact]
        public async Task Handle_UserNotAdmin_Returns_Forbidden()
        {
            var (userId, clubId) = SetupUser();
            _clubAuthorizationService.Setup(r => r.IsUserAdminOfClubAsync(userId, clubId)).ReturnsAsync(false);

            var behavior = CreateBehavior<TestRequestRequireClubAdmin>();

            var result = await behavior.Handle(
                new TestRequestRequireClubAdmin { ClubId = clubId },
                _ => Next(),
                CancellationToken.None);

            result.IsError.Should().BeTrue();
            result.TopError.Type.Should().Be(ErrorKind.Forbidden);
        }

        [Fact]
        public async Task Handle_UserIsAdmin_AllowsProcessing()
        {
            var (userId, clubId) = SetupUser();
            _clubAuthorizationService.Setup(r => r.IsUserAdminOfClubAsync(userId, clubId)).ReturnsAsync(true);

            var behavior = CreateBehavior<TestRequestRequireClubAdmin>();

            var result = await behavior.Handle(
                new TestRequestRequireClubAdmin { ClubId = clubId },
                _ => Next(),
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_SuperAdmin_BypassesAdminCheck()
        {
            var (userId, clubId) = SetupUser(roles: new[] { "SuperAdmin" });
            var behavior = CreateBehavior<TestRequestRequireClubAdmin>();

            var result = await behavior.Handle(
                new TestRequestRequireClubAdmin { ClubId = clubId },
                _ => Next(),
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();

            _clubAuthorizationService.Verify(r => r.IsUserAdminOfClubAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task Handle_MemberNotInClub_Returns_Forbidden()
        {
            var (userId, clubId) = SetupUser();
            _clubAuthorizationService.Setup(r => r.IsUserMemberOfClubAsync(userId, clubId)).ReturnsAsync(false);

            var behavior = CreateBehavior<TestRequestRequireMembership>();

            var result = await behavior.Handle(
                new TestRequestRequireMembership { ClubId = clubId },
                _ => Next(),
                CancellationToken.None);

            result.IsError.Should().BeTrue();
            result.TopError.Type.Should().Be(ErrorKind.Forbidden);
        }

        [Fact]
        public async Task Handle_OwnershipFails_Returns_Forbidden()
        {
            var (userId, clubId) = SetupUser();
            _clubAuthorizationService.Setup(r => r.IsUserAdminOfClubAsync(userId, clubId)).ReturnsAsync(true);
            _clubAdminUserRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ClubAdminUser.Create(userId, clubId, "First", "Last", null, null));

            var behavior = CreateBehavior<TestOwnershipRequest>();

            var result = await behavior.Handle(
                new TestOwnershipRequest { ClubId = clubId },
                _ => Next(),
                CancellationToken.None);

            result.IsError.Should().BeTrue();
            result.TopError.Type.Should().Be(ErrorKind.Forbidden);
        }

        [Fact]
        public async Task Handle_SuperAdmin_OwnershipFails_AllowsProcessing()
        {
            // Arrange: SuperAdmin user
            var (userId, clubId) = SetupUser(roles: new[] { "SuperAdmin" });

            // Setup repository to return a ClubAdminUser (needed for normal ownership checks)
            _clubAdminUserRepository
                .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ClubAdminUser.Create(userId, clubId, "First", "Last", null, null));

            var behavior = CreateBehavior<TestOwnershipRequest>();

            // Act: Execute the pipeline
            var result = await behavior.Handle(
                new TestOwnershipRequest { ClubId = clubId },
                _ => Next(),
                CancellationToken.None);

            // Assert: SuperAdmin bypasses ownership validation → should succeed
            result.IsSuccess.Should().BeTrue();
        }

        #endregion
    }
}