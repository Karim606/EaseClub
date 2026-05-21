using EaseClub.Domain.ApplicationTemplates.ValueObjects.ConditionExpression;
using EaseClub.Domain.MembershipApplications.ValueObjects;
using EaseClub.Domain.PricingPolices;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Tests.Entities.PricingPolicies
{
    public class PricingEngineTests
    {
        private readonly PricingContext _emptyContext = new(new Dictionary<string, string?>());

        [Fact]
        public void Calculate_ShouldApplyFixedIncrease_WhenPolicyIsSimple()
        {
            // Arrange
            var basePrice = 100m;
            var policy = CreateMockPolicy("Fee", priority: 1, isIncrease: true, fixedAmount: 15m);

            // Act
            var result = PricingEngine.Calculate(basePrice, new[] { policy }, _emptyContext);

            // Assert
            result.TotalPrice.Should().Be(115m);
            result.AppliedPolicies.Should().ContainSingle(p => p.Name == "Fee" && p.Adjustment == 15m);
        }

        [Fact]
        public void Calculate_ShouldApplyPercentage_BasedOnBasePrice()
        {
            // Arrange
            var basePrice = 200m;
            var policy = CreateMockPolicy("Discount", priority: 1, isIncrease: false, percentage: 0.10m); // 10%

            // Act
            var result = PricingEngine.Calculate(basePrice, new[] { policy }, _emptyContext);

            // Assert
            result.TotalPrice.Should().Be(180m); // 200 - 20
        }

        [Fact]
        public void Calculate_ShouldRespectPriority_WhenApplyingMixedPolicies()
        {
            // Arrange
            var basePrice = 100m;
            // Even if added to the list out of order, priority 1 should run before priority 2
            var p2 = CreateMockPolicy("Late Fee", priority: 2, isIncrease: true, fixedAmount: 10m);
            var p1 = CreateMockPolicy("Early Discount", priority: 1, isIncrease: false, fixedAmount: 20m);

            // Act
            var result = PricingEngine.Calculate(basePrice, new[] { p2, p1 }, _emptyContext);

            // Assert
            // First adjustment: 100 - 20 = 80. Second adjustment: 80 + 10 = 90.
            result.TotalPrice.Should().Be(90m);
            result.AppliedPolicies[0].Name.Should().Be("Early Discount");
            result.AppliedPolicies[1].Name.Should().Be("Late Fee");
        }

        [Fact]
        public void Calculate_ShouldApplyMultiplier_WhenKeyExistsInContext()
        {
            // Arrange
            var basePrice = 100m;
            var policy = CreateMockPolicy("Guest Fee", priority: 1, isIncrease: true, fixedAmount: 10m, multiplierKey: "guests");
            var context = new PricingContext(new Dictionary<string, string?> { { "guests", "3" } });

            // Act
            var result = PricingEngine.Calculate(basePrice, new[] { policy }, context);

            // Assert
            result.TotalPrice.Should().Be(130m); // 100 + (10 * 3)
        }

        [Fact]
        public void GetRequiredContextKeys_ShouldReturnAllUniqueKeys()
        {
            // Arrange
            var p1 = CreateMockPolicy("P1", multiplierKey: "key1");
            var condition = ConditionExpression.Create("key2", ComparisonOperator.Equals, "Value").Value;

            var p2 = CreateMockPolicy("P2", conditions: new List<ConditionExpression> { condition });

            // Act
            var keys = PricingEngine.GetRequiredContextKeys(new[] { p1, p2 });

            // Assert
            keys.Should().HaveCount(2);
            keys.Should().Contain(new[] { "key1", "key2" });
        }

        // Helper to create snapshots for testing without hitting the DB
        private IPricingPolicy CreateMockPolicy(
            string name,
            int priority = 1,
            bool isIncrease = true,
            decimal? fixedAmount = null,
            decimal? percentage = null,
            string? multiplierKey = null,
            List<ConditionExpression>? conditions = null)
        {
            return new PricingPolicySnapshot(Guid.NewGuid(), name, priority, isIncrease, fixedAmount, percentage, multiplierKey, conditions);
        }
    }
}
