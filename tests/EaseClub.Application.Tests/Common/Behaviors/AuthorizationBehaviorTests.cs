using EaseClub.Application.Common.Behaviors;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Tests.Common.Behaviors.sharedSetupForTests;
using EaseClub.Domain.ClubAdmin;
using EaseClub.Domain.Common;
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
        private readonly Mock<IClubAuthorizationService> clubAuthorizationService=new();
        private readonly Mock<ICurrentUserService> currentUserService = new();
        private readonly Mock<ILogger<AuthorizationBehavior<TestRequestRequireClubAdmin, Result<TestResponse>>>> logger = new();

        public class TestRequestRequireClubAdmin : IRequireClubAdmin
        {
            public Guid ClubId { get; init; }
        }
       
        private AuthorizationBehavior<TestRequestRequireClubAdmin, Result<TestResponse>> CreateBehavior()
            => new(
                clubAuthorizationService.Object,
                currentUserService.Object,
                logger.Object
            );

        //[Fact]
        //public async Task Handle_UserNotFound_Returns_Unauthorized()
        //{
        //    // Arrange
        //    var userId = Guid.NewGuid();
        //    var clubId = Guid.NewGuid();
        //    currentUserService.Setup(c => c.GetId()).Returns(userId.ToString());
        //    clubAuthorizationService.Setup(r => r.IsUserAdminOfClubAsync(userId, clubId))
        //                            .ReturnsAsync(false);

        //    var behavior = CreateBehavior();
        //    var request = new TestRequestRequireClubAdmin();
        //    // Act
        //    var result = await behavior.Handle(request, (CancellationToken) => Task.FromResult((Result<TestResponse>)new TestResponse()), CancellationToken.None);
        //    // Assert
        //    result.IsError.Should().BeTrue();
        //    result.TopError.Type.Should().Be(Domain.Common.ErrorKind.Unauthorized);
        //}

        [Fact]
        public async Task Handle_UserNotAuthorizedForClub_Returns_Forbidden()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var requestClubId = Guid.NewGuid();
          
            var behavior = CreateBehavior();
            var request = new TestRequestRequireClubAdmin
            {
                ClubId = requestClubId
            };
            currentUserService.Setup(c => c.GetId()).Returns(userId.ToString());
            clubAuthorizationService.Setup(r => r.IsUserAdminOfClubAsync(userId,requestClubId))
                                   .ReturnsAsync(false);
            // Act
            var result = await behavior.Handle(request, (CancellationToken) => Task.FromResult((Result<TestResponse>)new TestResponse()), CancellationToken.None);

            //Assert
            result.IsError.Should().BeTrue();
            result.TopError.Type.Should().Be(ErrorKind.Forbidden);
        }

        [Fact]
        public async Task Handle_UserAuthorizedForClub_AllowsProcessing()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var authorizedClubId = Guid.NewGuid();

            var behavior = CreateBehavior();
            var request = new TestRequestRequireClubAdmin
            {
                ClubId = authorizedClubId
            };
            currentUserService.Setup(c => c.GetId()).Returns(userId.ToString());
            clubAuthorizationService.Setup(r => r.IsUserAdminOfClubAsync(userId,authorizedClubId))
                                   .ReturnsAsync(true);
            // Act
            var result = await behavior.Handle(request, (CancellationToken) => Task.FromResult((Result<TestResponse>)new TestResponse()), CancellationToken.None);

            //Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeOfType<TestResponse>();
        }


    }
}