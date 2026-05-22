using EaseClub.Domain.Clubs;
using EaseClub.Domain.Clubs.ValueObjects;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Common.ValueObjects;
using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;

namespace EaseClub.Domain.Tests.Entities
{
    public class ClubTests
    {
        private static Result<Club> CreateClub(Guid id, string name)
        {
            var phone = PhoneNumber.Create("01012345678").Value;
            var email = Email.Create("test@example.com").Value;
            var contactInfo = new ContactInfo(phone, email);

            var schedules = new List<WorkSchedule>
            {
                WorkSchedule.Create("Daily", "6:00 AM - 10:00 PM").Value
            };

            var amenities = new List<Amenity> { new("Pool") };

            return Club.Create(
                id: id,
                name: name,
                about: "About",
                contactInfo: contactInfo,
                workSchedules: schedules,
                amenities: amenities,
                code: "EASE");
        }

        private static Club CreateValidClub(Guid id, string name) => CreateClub(id, name).Value;

        [Fact]
        public void Create_Should_Fail_When_Name_Is_Null_Or_Whitespace()
        {
            var id = Guid.NewGuid();

            var result = CreateClub(id, " ");

            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(ClubErrors.NullOrWhiteSpaces);
        }

        [Theory]
        [InlineData("ab")] // < 3 chars
        [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")] // > 100 chars
        public void Create_Should_Fail_When_Name_Length_Is_Invalid(string name)
        {
            var id = Guid.NewGuid();

            var result = CreateClub(id, name);

            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(ClubErrors.Name_Length_NotSuitable);
        }

        [Fact]
        public void Create_Should_Succeed_When_Valid_Data_Is_Provided()
        {
            var id = Guid.NewGuid();
            var name = "Ease Club";

            var result = CreateClub(id, name);

            result.IsSuccess.Should().BeTrue();
            result.Value.Id.Should().Be(id);
            result.Value.Name.Should().Be(name);
            result.Value.IsActive.Should().BeTrue();
        }

        [Fact]
        public void Deactivate_Should_Set_IsActive_To_False()
        {
            var club = CreateValidClub(Guid.NewGuid(), "Ease Club");

            club.Deactivate();

            club.IsActive.Should().BeFalse();
        }

        [Fact]
        public void Activate_Should_Set_IsActive_To_True()
        {
            var club = CreateValidClub(Guid.NewGuid(), "Ease Club");
            club.Deactivate();

            club.Activate();

            club.IsActive.Should().BeTrue();
        }

        [Fact]
        public void New_Club_Should_Have_No_Branches_And_No_Admins()
        {
            var club = CreateValidClub(Guid.NewGuid(), "Ease Club");

            club.Branches.Should().NotBeNull().And.BeEmpty();
            club.ClubAdmins.Should().NotBeNull().And.BeEmpty();
        }
    }
}

