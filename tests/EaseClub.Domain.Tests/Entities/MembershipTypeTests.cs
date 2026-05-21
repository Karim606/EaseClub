using EaseClub.Domain.MembershipTypes;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Tests.Entities
{
    public class MembershipTypeTests
    {
        private static Guid ClubId = Guid.NewGuid();
        private static Guid MembershipTypeId = Guid.NewGuid();

        #region Create

        [Fact]
        public void Create_Should_Fail_When_ClubId_Is_Empty()
        {
            // Act
            var result = MembershipType.Create(
                MembershipTypeId,
                Guid.Empty,
                "Gold");

            // Assert
            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(MembershipTypeErrors.ClubIdIsRequired);
        }

        [Fact]
        public void Create_Should_Fail_When_Name_Is_Empty()
        {
            var result = MembershipType.Create(
                MembershipTypeId,
                ClubId,
                "");

            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(MembershipTypeErrors.MembershipTypeNameMustNotBeEmpty);
        }

        [Fact]
        public void Create_Should_Succeed_With_Valid_Data()
        {
            var result = MembershipType.Create(
                MembershipTypeId,
                ClubId,
                "Gold");

            result.IsSuccess.Should().BeTrue();
            result.Value.Name.Should().Be("Gold");
            result.Value.ClubId.Should().Be(ClubId);
            result.Value.IsActive.Should().BeTrue();
            result.Value.AllBranchesPermitted.Should().BeTrue();
        }

        #endregion


        #region RestrictToBranches

        [Fact]
        public void RestrictToBranches_Should_Fail_When_List_Is_Empty()
        {
            var type = CreateValidType();

            var result = type.RestrictToBranches([]);

            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(MembershipTypeErrors.RestrictedToBranchListMustBeGreaterThanZero);
        }

        [Fact]
        public void RestrictToBranches_Should_Add_Unique_Branches()
        {
            var type = CreateValidType();

            var branch1 = Guid.NewGuid();
            var branch2 = Guid.NewGuid();

            var result = type.RestrictToBranches([branch1, branch2, branch1]);

            result.IsSuccess.Should().BeTrue();
            type.AllBranchesPermitted.Should().BeFalse();
            type.PermittedBranches.Should().HaveCount(2);
            type.PermittedBranches.Select(b => b.BranchId)
                .Should().BeEquivalentTo(new[] { branch1, branch2 });
        }

        #endregion

        #region PermitAllBranchesAccess

        [Fact]
        public void PermitAllBranchesAccess_Should_Fail_When_Already_Permitted()
        {
            var type = CreateValidType();

            var result = type.PermitAllBranchesAccess();

            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(MembershipTypeErrors.MembershipTypeAlreadyAllBranchesPermitted);
        }

        [Fact]
        public void PermitAllBranchesAccess_Should_Clear_Restrictions()
        {
            var type = CreateValidType();
            type.RestrictToBranches([Guid.NewGuid()]);

            var result = type.PermitAllBranchesAccess();

            result.IsSuccess.Should().BeTrue();
            type.AllBranchesPermitted.Should().BeTrue();
            type.PermittedBranches.Should().BeEmpty();
        }

        #endregion

        #region Helpers

        private static MembershipType CreateValidType()
        {
            return MembershipType
                .Create(MembershipTypeId, ClubId, "Gold")
                .Value;
        }

        #endregion
    }
}
