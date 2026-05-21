using EaseClub.Domain.MembershipPlans;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Tests.Entities.MembershipPlan
{
    public class InstallmentEngineTests
    {
        [Fact]
        public void GenerateMembershipInstallments_ShouldCalculateCorrectAmounts_AndMatchTotal()
        {
            // Arrange
            var totalPrice = 1000m;
            var rules = new List<Installment>
        {
            Installment.Create(50m, 30, 1).Value, // 50% due in 30 days
            Installment.Create(50m, 30, 2).Value  // 50% due 30 days after that
        };

            // Act
            var result = InstallmentEngine.GenerateMembershipInstallments(rules, totalPrice);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(2);
            result.Value.Sum(x => x.Amount).Should().Be(totalPrice);
            result.Value[0].Amount.Should().Be(500m);
            result.Value[1].Amount.Should().Be(500m);
        }

        [Fact]
        public void GenerateMembershipInstallments_ShouldHandleRemainderCorrectly_ForUnevenPercentages()
        {
            // Example: 33.33% + 33.33% + 33.34% = 100% of 1000.00
            var totalPrice = 1000m;
            var rules = new List<Installment>
        {
            Installment.Create(33.33m, 30, 1).Value,
            Installment.Create(33.33m, 30, 2).Value,
            Installment.Create(33.34m, 30, 3).Value
        };

            var result = InstallmentEngine.GenerateMembershipInstallments(rules, totalPrice);

            result.IsSuccess.Should().BeTrue();
            result.Value.Sum(x => x.Amount).Should().Be(totalPrice);
        }

        [Fact]
        public void GenerateMembershipInstallments_ShouldFail_WhenPercentagesDoNotSumTo100()
        {
            var rules = new List<Installment>
        {
            Installment.Create(50m, 30, 1).Value,
            Installment.Create(40m, 30, 2).Value // Only 90%
        };

            var result = InstallmentEngine.GenerateMembershipInstallments(rules, 1000m);

            result.IsError.Should().BeTrue();
            result.TopError.Code.Should().Be("Installments.InvalidPercentage");
        }

        [Fact]
        public void GenerateMembershipInstallments_ShouldFail_WhenOrderIsNotSequential()
        {
            var rules = new List<Installment>
        {
            Installment.Create(50m, 30, 1).Value,
            Installment.Create(50m, 30, 3).Value // Missing 2
        };

            var result = InstallmentEngine.GenerateMembershipInstallments(rules, 1000m);

            result.IsError.Should().BeTrue();
            result.TopError.Code.Should().Be("Installments.InvalidOrder");
        }
    }
}
