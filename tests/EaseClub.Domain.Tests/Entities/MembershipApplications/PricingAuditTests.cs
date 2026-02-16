using EaseClub.Domain.MembershipApplications;
using EaseClub.Domain.MembershipApplications.Errors;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Tests.Entities.MembershipApplications
{
    public class PricingAuditTests
    {
        private readonly Guid _appId = Guid.NewGuid();
        private readonly Guid _policyId = Guid.NewGuid();

        [Fact]
        public void Create_ShouldReturnSuccess_WhenPricesAreDifferent()
        {
            // Act
            var result = PricingAudit.Create(Guid.NewGuid(), _appId, _policyId, 100m, 120m);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.OldPrice.Should().Be(100m);
            result.Value.NewPrice.Should().Be(120m);
            result.Value.AppliedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Create_ShouldFail_WhenOldAndNewPriceAreSame()
        {
            // Act
            var result = PricingAudit.Create(Guid.NewGuid(), _appId, _policyId, 150m, 150m);

            // Assert
            result.IsError.Should().BeTrue();
            // Since this is a logic conflict, you might want to ensure this is Error.Conflict
            result.TopError.Should().Be(PricingAuditErrors.NoPriceChange);
        }

        [Theory]
        [InlineData(-1, 100)]
        [InlineData(100, -5)]
        public void Create_ShouldFail_WhenPricesAreNegative(decimal oldP, decimal newP)
        {
            // Act
            var result = PricingAudit.Create(Guid.NewGuid(), _appId, _policyId, oldP, newP);

            // Assert
            result.IsError.Should().BeTrue();
        }
    }
}
