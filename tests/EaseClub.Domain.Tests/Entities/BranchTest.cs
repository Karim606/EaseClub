using EaseClub.Domain.Branches;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Tests.Entities
{
    public class BranchTests
    {
        [Fact]
        public void Create_Should_Fail_When_Name_Is_Null_Or_Whitespace()
        {
            // Arrange
            var id = Guid.NewGuid();
            var clubId = Guid.NewGuid();

            // Act
            var result = Branch.Create(id, clubId, " ");

            // Assert
            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(BranchErrors.NullOrWhiteSpaces); ;
        }

        [Theory]
        [InlineData("ab")] // less than 3 chars
        [InlineData(
            "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"
        )] // more than 100 chars
        public void Create_Should_Fail_When_Name_Length_Is_Invalid(string name)
        {
            // Arrange
            var id = Guid.NewGuid();
            var clubId = Guid.NewGuid();

            // Act
            var result = Branch.Create(id, clubId, name);

            // Assert
            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(BranchErrors.Name_Length_NotSuitable);
        }

        [Fact]
        public void Create_Should_Succeed_When_Valid_Data_Is_Provided()
        {
            // Arrange
            var id = Guid.NewGuid();
            var clubId = Guid.NewGuid();
            var name = "Main Branch";

            // Act
            var result = Branch.Create(id, clubId, name);

            // Assert
            result.IsSuccess.Should().BeTrue();

            result.Value.Should().NotBeNull();
            result.Value.Id.Should().Be(id);
            result.Value.ClubId.Should().Be(clubId);
            result.Value.Name.Should().Be(name);
            result.Value.IsActive.Should().BeTrue();
        }

        [Fact]
        public void Deactivate_Should_Set_IsActive_To_False()
        {
            // Arrange
            var branch = Branch.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Downtown Branch"
            ).Value;

            // Act
            branch.Deactivate();

            // Assert
            branch.IsActive.Should().BeFalse();
        }

        [Fact]
        public void Activate_Should_Set_IsActive_To_True()
        {
            // Arrange
            var branch = Branch.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Downtown Branch"
            ).Value;

            branch.Deactivate();

            // Act
            branch.Activate();

            // Assert
            branch.IsActive.Should().BeTrue();
        }
    }
}
