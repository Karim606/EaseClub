using EaseClub.Domain.Memberships;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Tests.Entities.Memberships
{
    public class FamilyMemberTests
    {
        [Theory]
        [InlineData("1990-01-01", 36, false)] // Adult
        [InlineData("2015-01-01", 11, true)]  // Minor
        public void IsMinor_ShouldReturnCorrectResult(string dob, int expectedAge, bool expectedIsMinor)
        {
            var member = FamilyMember.Create(
                Guid.NewGuid(),
                "Test Member",
                FamilyRelationship.Son,
                DateOnly.Parse(dob)
            ).Value;

            member.GetAge(new DateOnly(2026, 3, 13)).Should().Be(expectedAge);
            member.IsMinor(new DateOnly(2026, 3, 13)).Should().Be(expectedIsMinor);
        }

        [Fact]
        public void Create_ShouldFail_WhenFullNameIsEmpty()
        {
            var result = FamilyMember.Create(Guid.NewGuid(), "", FamilyRelationship.Wife, default);
            result.IsError.Should().BeTrue();
            result.TopError.Code.Should().Be("FamilyMember.InvalidName");
        }
    }
}
