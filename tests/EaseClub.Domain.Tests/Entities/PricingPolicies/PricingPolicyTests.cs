using EaseClub.Domain.PricingPolices;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Tests.Entities.PricingPolicies
{
    public class PricingPolicyTests
    {
        [Fact]
        public void Create_ShouldReturnError_WhenBothFixedAndPercentageProvided()
        {
            // Act
            var result = PricingPolicy.Create(
                id: Guid.NewGuid(),
                clubId: Guid.NewGuid(),
                name: "Invalid",
                isIncrease: true,
                fixedAmount: 10m,
                percentageValue: 0.1m,
                multiplierKey: null);

            // Assert
            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(PricingPolicyErrors.CantCombinePercentageAndFixedAmount);
        }

        [Fact]
        public void ToSnapshot_ShouldMapAllFieldsCorrectly()
        {
            // Arrange
            var policy = PricingPolicy.Create(
                id: Guid.NewGuid(),
                clubId: Guid.NewGuid(),
                name: "VAT",
                isIncrease: true,
                fixedAmount: null,
                percentageValue: 0.15m,
                multiplierKey: null).Value;

            // Act
            var snapshot = policy.ToSnapshot(priority: 100);

            // Assert
            snapshot.Name.Should().Be("VAT");
            snapshot.PercentageValue.Should().Be(0.15m);
            snapshot.IsIncrease.Should().BeTrue();
        }
    }
}
