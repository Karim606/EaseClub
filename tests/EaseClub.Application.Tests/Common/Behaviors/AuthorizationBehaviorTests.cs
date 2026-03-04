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
        private readonly Mock<ILogger<AuthorizationBehavior<IAuthorizeRequest, Result<TestResponse>>>> _logger = new();

        #region Test Request Types

        public class TestResponse : IResult
        {
            public bool IsSuccess => true;
            public bool IsError => false;
            public IReadOnlyList<Error>? Errors => null;
        }

        public class TestRequestRequireClubAdmin : IRequireClubAdmin
        {
            public Guid ClubId { get; init; }
        }

        public class TestRequestRequireMembership : IRequireMembership
        {
            public Guid ClubId { get; init; }
        }

        public class TestOwnershipRequest :
            IRequireClubAdmin,
            IRequireClubOwnershipValidation
        {
            public Guid ClubId { get; init; }

            public IEnumerable<OwnershipRule> Rules()
            {
                yield return new OwnershipRule(
                    (auth, clubId) => Task.FromResult(false), // force failure
                    "Field",
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
                Mock.Of<ILogger<AuthorizationBehavior<TRequest, Result<TestResponse>>>>());
        }

        private Task<Result<TestResponse>> Next()
            => Task.FromResult((Result<TestResponse>)new TestResponse());

        private (Guid userId, Guid clubId) SetupUser(
            IEnumerable<string>? roles = null,
            string? userIdValue = null)
        {
            var userId = Guid.NewGuid();
            var clubId = Guid.NewGuid();

            _currentUserService.Setup(c => c.GetId()).Returns(userIdValue ?? userId.ToString());
            _currentUserService.Setup(c => c.GetRoles()).Returns(roles is null ? new List<string>() : new List<string>(roles));

            return (userId, clubId);
        }

        private AuthorizationBehavior<TRequest, Result<TestResponse>> SetupBehavior<TRequest>()
            where TRequest : notnull, IAuthorizeRequest
            => CreateBehavior<TRequest>();

        #endregion

        #region Tests

        [Fact]
        public async Task Handle_UserIdEmpty_Returns_Unauthorized()
        {
            _currentUserService.Setup(c => c.GetId()).Returns(string.Empty);

            var behavior = SetupBehavior<TestRequestRequireClubAdmin>();

            var result = await behavior.Handle(
                new TestRequestRequireClubAdmin { ClubId = Guid.NewGuid() },
                _ => Next(),
                CancellationToken.None);

            result.IsError.Should().BeTrue();
            result.TopError.Type.Should().Be(ErrorKind.Unauthorized);
        }

        [Fact]
        public async Task Handle_UserNotAuthorizedForClub_Returns_Forbidden()
        {
            var (userId, clubId) = SetupUser();

            _clubAuthorizationService
                .Setup(r => r.IsUserAdminOfClubAsync(userId, clubId))
                .ReturnsAsync(false);

            var behavior = SetupBehavior<TestRequestRequireClubAdmin>();

            var result = await behavior.Handle(
                new TestRequestRequireClubAdmin { ClubId = clubId },
                _ => Next(),
                CancellationToken.None);

            result.IsError.Should().BeTrue();
            result.TopError.Type.Should().Be(ErrorKind.Forbidden);
        }

        [Fact]
        public async Task Handle_UserAuthorizedForClub_AllowsProcessing()
        {
            var (userId, clubId) = SetupUser();

            _clubAuthorizationService
                .Setup(r => r.IsUserAdminOfClubAsync(userId, clubId))
                .ReturnsAsync(true);

            var behavior = SetupBehavior<TestRequestRequireClubAdmin>();

            var result = await behavior.Handle(
                new TestRequestRequireClubAdmin { ClubId = clubId },
                _ => Next(),
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_SuperAdmin_Bypasses_AdminCheck()
        {
            var (userId, clubId) = SetupUser(roles: new[] { "SuperAdmin" });

            var behavior = SetupBehavior<TestRequestRequireClubAdmin>();

            var result = await behavior.Handle(
                new TestRequestRequireClubAdmin { ClubId = clubId },
                _ => Next(),
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();

            _clubAuthorizationService.Verify(
                s => s.IsUserAdminOfClubAsync(It.IsAny<Guid>(), It.IsAny<Guid>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_MemberNotInClub_Returns_Forbidden()
        {
            var (userId, clubId) = SetupUser();

            _clubAuthorizationService
                .Setup(s => s.IsUserMemberOfClubAsync(userId, clubId))
                .ReturnsAsync(false);

            var behavior = SetupBehavior<TestRequestRequireMembership>();

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

            _clubAuthorizationService
                .Setup(r => r.IsUserAdminOfClubAsync(userId, clubId))
                .ReturnsAsync(true);

            var behavior = SetupBehavior<TestOwnershipRequest>();

            var result = await behavior.Handle(
                new TestOwnershipRequest { ClubId = clubId },
                _ => Next(),
                CancellationToken.None);

            result.IsError.Should().BeTrue();
            result.TopError.Type.Should().Be(ErrorKind.Forbidden);
        }

        [Fact]
        public async Task Handle_SuperAdmin_StillFails_Ownership()
        {
            var (userId, clubId) = SetupUser(roles: new[] { "SuperAdmin" });

            var behavior = SetupBehavior<TestOwnershipRequest>();

            var result = await behavior.Handle(
                new TestOwnershipRequest { ClubId = clubId },
                _ => Next(),
                CancellationToken.None);

            result.IsError.Should().BeTrue();
            result.TopError.Type.Should().Be(ErrorKind.Forbidden);
        }

        #endregion
    }
}