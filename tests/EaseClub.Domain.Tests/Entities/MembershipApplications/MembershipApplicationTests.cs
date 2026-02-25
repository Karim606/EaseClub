using EaseClub.Domain.Common;
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
    public class MembershipApplicationTests
    {
        private readonly Guid _userId = Guid.NewGuid();
        private readonly Guid _clubId = Guid.NewGuid();
        private readonly Guid _typeId = Guid.NewGuid();
        private readonly Guid _planId = Guid.NewGuid();
        private readonly Guid _templateId = Guid.NewGuid();
        private const string _dummySnapshot = "{\"Steps\":[]}"; // Minimal valid JSON for testing

        [Fact]
        public void Create_ShouldInitializeWithDraftStatusAndEstimatedPricing()
        {
            // Act
            var result = MembershipApplication.Create(
                Guid.NewGuid(), "TRK-123", _dummySnapshot, _userId, _clubId, _typeId, _planId, _templateId, 100.00m);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Status.Should().Be(ApplicationStatus.Draft);
            result.Value.PricingState.Should().Be(PricingState.Estimated);
            result.Value.FinalPrice.Should().Be(100.00m);
            result.Value.TemplateSnapshot.Should().Be(_dummySnapshot);
        }

        [Fact]
        public void Submit_ShouldLockPricingAndSetTimestamp()
        {
            // Arrange
            var app = MembershipApplication.Create(
                Guid.NewGuid(), "TRK-123", _dummySnapshot, _userId, _clubId, _typeId, _planId, _templateId, 100.00m).Value;

            // Act
            var result = app.Submit();

            // Assert
            result.IsSuccess.Should().BeTrue();
            app.Status.Should().Be(ApplicationStatus.Submitted);
            app.PricingState.Should().Be(PricingState.Locked);
            app.SubmittedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void ApplyPricing_ShouldReturnError_WhenPricingIsLocked()
        {
            // Arrange
            var app = MembershipApplication.Create(
                Guid.NewGuid(), "TRK-123", _dummySnapshot, _userId, _clubId, _typeId, _planId, _templateId, 100.00m).Value;
            app.Submit(); // This sets PricingState to Locked

            // Act
            var result = app.ApplyPricing(150.00m);

            // Assert
            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(MembershipApplicationErrors.PricingLocked);
        }
    }
}
