using EaseClub.Domain.ApplicationTemplates.ValueObjects.ConditionExpression;
using EaseClub.Domain.Common.ValueObjects;
using EaseClub.Domain.MembershipApplications;
using EaseClub.Domain.MembershipApplications.Enums;
using EaseClub.Domain.MembershipApplications.Errors;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Tests.Entities.MembershipApplications
{
    public class PricingPolicyTests
    {
        // Dummy condition for testing
        private readonly ConditionExpression _dummyCondition = ConditionExpression.Create("age", ComparisonOperator.GreaterThan, "18").Value;

        [Theory]
        [InlineData(100, 10, true, 110)]  // $10 increase
        [InlineData(100, 10, false, 90)]  // $10 decrease
        public void Apply_ShouldCalculateFlatAmount_Correctly(decimal current, decimal effect, bool isIncrease, decimal expected)
        {
            // Arrange
            var policy = PricingPolicy.Create(
                Guid.NewGuid(), PricingTrigger.OnFieldChange, _dummyCondition, "Flat Fee",
                Money.Of(effect, "USD").Value, isPercentage: false, isIncrease: isIncrease).Value;

            // Act
            var result = policy.Apply(current);

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(200, 10, true, 220)]  // 10% increase of 200 = 220
        [InlineData(200, 5, false, 190)]  // 5% decrease of 200 = 190
        public void Apply_ShouldCalculatePercentage_Correctly(decimal current, decimal effect, bool isIncrease, decimal expected)
        {
            // Arrange
            var policy = PricingPolicy.Create(
                Guid.NewGuid(), PricingTrigger.OnFieldChange, _dummyCondition, "Tax/Discount",
                Money.Of(effect, "USD").Value, isPercentage: true, isIncrease: isIncrease).Value;

            // Act
            var result = policy.Apply(current);

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void Create_ShouldFail_WhenEffectAmountIsZeroOrNegative()
        {
            // Act
            var result = PricingPolicy.Create(
                Guid.NewGuid(), PricingTrigger.OnFieldChange, _dummyCondition, "Free",
                Money.Of(0, "USD").Value, false, false);

            // Assert
            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(PricingPolicyErrors.InvalidEffectAmount);
        }
    }
}
