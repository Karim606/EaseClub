using EaseClub.Application.Features.Branches.Queries.GetBranchesByClub;
using EaseClub.Application.Tests.Common.Behaviors;
using EaseClub.Domain.Branches;
using EaseClub.Domain.Common;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Tests.Features.Branches.Queries
{
    public class GetBranchesByClubQueryHandlerTests
    {
        private readonly Mock<IBranchRepository> _branchRepository = new();
        private readonly Mock<ILogger<GetBranchesByClubQueryHandler>> _logger = new();

        private GetBranchesByClubQueryHandler CreateHandler()
            => new(_branchRepository.Object, _logger.Object);

        [Fact]
        public async Task Handle_Should_Return_EmptyList_When_No_Branches()
        {
            // Arrange
            var clubId = Guid.NewGuid();
            _branchRepository.Setup(x => x.GetBranchesByClubIdAsync(clubId, It.IsAny<bool?>(), It.IsAny<CancellationToken>()))
                             .ReturnsAsync(new List<Branch>());

            var handler = CreateHandler();
            var query = new GetBranchesByClubQuery(clubId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEmpty();

        }

        [Fact]
        public async Task Handle_Should_Return_Branches_When_Exist()
        {
            // Arrange
            var clubId = Guid.NewGuid();
            var branches = new List<Branch>
            {
                Branch.Create(Guid.NewGuid(), clubId, "Branch 1", "Address 1").Value,
                Branch.Create(Guid.NewGuid(), clubId, "Branch 2", "Address 2").Value
            };

            _branchRepository.Setup(x => x.GetBranchesByClubIdAsync(clubId, It.IsAny<bool?>(), It.IsAny<CancellationToken>()))
                             .ReturnsAsync(branches);

            var handler = CreateHandler();
            var query = new GetBranchesByClubQuery(clubId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(2);
            result.Value.Should().ContainSingle(b => b.Name == "Branch 1");
            result.Value.Should().ContainSingle(b => b.Name == "Branch 2");
        }
    }
}
