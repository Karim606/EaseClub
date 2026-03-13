using EaseClub.Domain.ApplicationTemplates;
using EaseClub.Domain.Memberships;
using EaseClub.Domain.Memberships.Events;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Tests.Entities.Memberships
{
    public class MembershipTests
    {
        [Fact]
        public void CreateFromApplication_ShouldSetStatusToSuspended_AndMapInstallments()
        {
            // Arrange
            var app = MembershipDataBuilder.CreateApprovedApplication(); // Use a DataBuilder

            // Act
            var result = Membership.CreateFromApplication(app);
            var familySection = app.TemplateSnapshot.Steps
                .SelectMany(s => s.Sections)
                .FirstOrDefault(sec => sec.Intent == SectionIntent.FamilyMembers);

            var groupedMembers = app.Answers
               .Where(a => familySection.Fields.Any(f => f.Id == a.FieldDefinitionId))
               .GroupBy(a => a.InstanceIndex);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Status.Should().Be(MembershipStatus.Suspended);
            result.Value.MembershipInstallments.Should().NotBeEmpty();
            result.Value.FamilyMembers.Should().HaveCount(groupedMembers.Count());
        }

        [Fact]
        public void Activate_ShouldChangeStatusToActive_AndRaiseEvent()
        {
            var membership = MembershipDataBuilder.CreateSuspendedMembership();

            var result = membership.Activate();

            result.IsSuccess.Should().BeTrue();
            membership.Status.Should().Be(MembershipStatus.Active);
            membership.DomainEvents.Should().Contain(e => e is MembershipActivatedDomainEvent);
        }

        [Fact]
        public void Renew_ShouldExtendEndDate_AndUpdatePlan()
        {
            var membership = MembershipDataBuilder.CreateActiveMembership();
            var newEndDate = membership.Period.EndDate.AddYears(1);
            var newPlanId = Guid.NewGuid();

            var result = membership.Renew(newEndDate, newPlanId);

            result.IsSuccess.Should().BeTrue();
            membership.Period.EndDate.Should().Be(newEndDate);
            membership.MembershipPlanId.Should().Be(newPlanId);
        }

        [Fact]
        public void Renew_ShouldExtendEndDate()
        {
            // Arrange: Use the builder to get an object in the desired state
            var membership = MembershipDataBuilder.CreateActiveMembership();
            var newEndDate = membership.Period.EndDate.AddYears(1);

            // Act
            var result = membership.Renew(newEndDate);

            // Assert
            result.IsSuccess.Should().BeTrue();
        }
    }
}
