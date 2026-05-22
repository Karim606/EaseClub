using EaseClub.Application.Features.Clubs.Queries.GetClubById;
using EaseClub.Domain.Clubs;
using EaseClub.Domain.Common;
using EaseClub.Domain.Files;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Clubs.ValueObjects;
using EaseClub.Domain.Common.ValueObjects;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Tests.Features.Clubs.Queries
{
    public class GetClubByIdQueryHandlerTests
    {
        private readonly Mock<IClubRepository> _clubRepository = new();
        private readonly Mock<ILogger<GetClubByIdQueryHandler>> _logger = new();
        private readonly Mock<IFileRepository> _fileRepository = new();
        private readonly Mock<IFileStorageService> _fileStorageService = new();

        private GetClubByIdQueryHandler CreateHandler()
            => new(_clubRepository.Object, _logger.Object, _fileRepository.Object, _fileStorageService.Object);

        [Fact]
        public async Task Handle_Should_Return_NotFound_When_Club_Does_Not_Exist()
        {
            // Arrange
            var clubId = Guid.NewGuid();
            _clubRepository.Setup(x => x.GetByIdAsync(clubId, CancellationToken.None))
                           .ReturnsAsync((Club)null); // Club not found

            var handler = CreateHandler();
            var query = new GetClubByIdQuery(clubId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.TopError.Type.Should().Be(ErrorKind.NotFound);

            
        }

        [Fact]
        public async Task Handle_Should_Return_ClubResponse_When_Club_Exists()
        {
            // Arrange
            var clubId = Guid.NewGuid();
            var phone = PhoneNumber.Create("01012345678").Value;
            var email = Email.Create("test@easeclub.com").Value;
            var contactInfo = new ContactInfo(phone, email);
            var workSchedules = new List<WorkSchedule> { WorkSchedule.Create("Monday - Friday", "06:00 AM - 10:00 PM").Value };
            var amenities = new List<Amenity> { new Amenity("Gym") };
            var club = Club.Create(
                clubId,
                "Ease Club",
                "Ease Club Description",
                contactInfo,
                workSchedules,
                amenities,
                "EC123"
            ).Value;

            _clubRepository.Setup(x => x.GetByIdAsync(clubId, CancellationToken.None))
                           .ReturnsAsync(club);

            var handler = CreateHandler();
            var query = new GetClubByIdQuery(clubId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Id.Should().Be(clubId);
            result.Value.Name.Should().Be("Ease Club");
        }
    }
}
