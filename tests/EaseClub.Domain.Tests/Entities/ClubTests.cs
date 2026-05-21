using EaseClub.Domain.Clubs;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Tests.Entities
{
    public class ClubTests
    {
        [Fact]
        public void Create_Should_Fail_When_Name_Is_Null_Or_Whitespace()
        {
            // Arrange
            var id = Guid.NewGuid();

            // Act
            var result = Club.Create(id, " ");

            // Assert
            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(ClubErrors.NullOrWhiteSpaces);
        }

        [Theory]
        [InlineData("ab")] // < 3 chars
        [InlineData(
            "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"
        )] // > 100 chars
        public void Create_Should_Fail_When_Name_Length_Is_Invalid(string name)
        {
            // Arrange
            var id = Guid.NewGuid();

            // Act
            var result = Club.Create(id, name);

            // Assert
            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(ClubErrors.Name_Length_NotSuitable);
        }

        [Fact]
        public void Create_Should_Succeed_When_Valid_Data_Is_Provided()
        {
            // Arrange
            var id = Guid.NewGuid();
            var name = "Ease Club";

            // Act
            var result = Club.Create(id, name);

            // Assert
            result.IsSuccess.Should().BeTrue();

            result.Value.Should().NotBeNull();
            result.Value.Id.Should().Be(id);
            result.Value.Name.Should().Be(name);
            result.Value.IsActive.Should().BeTrue();
        }

        [Fact]
        public void Deactivate_Should_Set_IsActive_To_False()
        {
            // Arrange
            var club = Club.Create(
                Guid.NewGuid(),
                "Ease Club"
            ).Value;

            // Act
            club.Deactivate();

            // Assert
            club.IsActive.Should().BeFalse();
        }

        [Fact]
        public void Activate_Should_Set_IsActive_To_True()
        {
            // Arrange
            var club = Club.Create(
                Guid.NewGuid(),
                "Ease Club"
            ).Value;

            club.Deactivate();

            // Act
            club.Activate();

            // Assert
            club.IsActive.Should().BeTrue();
        }

        [Fact]
        public void New_Club_Should_Have_No_Branches_And_No_Admins()
        {
            // Arrange & Act
            var club = Club.Create(
                Guid.NewGuid(),
                "Ease Club"
            ).Value;

            // Assert
            club.Branches.Should().NotBeNull().And.BeEmpty();
            club.ClubAdmins.Should().NotBeNull().And.BeEmpty();
        }
    }
}
