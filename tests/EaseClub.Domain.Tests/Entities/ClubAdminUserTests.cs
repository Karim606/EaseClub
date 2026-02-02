using EaseClub.Domain.ClubAdmin;
using EaseClub.Domain.Common.ValueObjects;
using EaseClub.Domain.Member;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Tests.Entities
{
    public class ClubAdminUserTests
    {
        [Fact]
        public void CreateClubAdminUser_ShouldReturnClubAdminUserInstance()
        {
            // Arrange
            var id = Guid.NewGuid();
            var clubId = Guid.NewGuid();
            var firstName = "John";
            var lastName = "Doe";
            var phoneNumber = PhoneNumber.Create("01018709552").Value;
            var email = Email.Create("mohamed@gmail.com").Value;

            // Act
            var user = ClubAdminUser.Create(id,clubId,firstName, lastName, phoneNumber, email);

            // Assert
            user.Should().NotBeNull();

        }
    }
}
