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
        private readonly Guid _planId = Guid.NewGuid();
        private readonly Guid _templateId = Guid.NewGuid();



        [Fact]
        public void Create_ShouldInitializeWithDraftStatusAndEstimatedPricing()
        {
            // Act
            var result = MembershipApplication.Create(
                Guid.NewGuid(), "TRK-123", _userId, _clubId, Guid.NewGuid(), _planId, _templateId, 100.00m);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Status.Should().Be(ApplicationStatus.Draft);
            result.Value.PricingState.Should().Be(PricingState.Estimated);
            result.Value.FinalPrice.Should().Be(100.00m);
        }

        [Fact]
        public void AddNewStepInstance_ShouldFail_WhenApplicationIsNotDraft()
        {
            // Arrange
            var app = MembershipApplication.Create(
                Guid.NewGuid(), "TRK-123", _userId, _clubId, Guid.NewGuid(), _planId, _templateId, 100.00m).Value;
            app.Submit(); // Move away from Draft

            // Act
            var result = app.AddNewStepInstance(Guid.NewGuid());

            // Assert
            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(MembershipApplicationErrors.CantModifyNonDraft);
        }

        [Fact]
        public void AddNewStepInstance_ShouldPreventDuplicateTemplateSteps()
        {
            // Arrange
            var stepId = Guid.NewGuid();
            var app = MembershipApplication.Create(
                Guid.NewGuid(), "TRK-123", _userId, _clubId, Guid.NewGuid(), _planId, _templateId, 100.00m).Value;
            app.AddNewStepInstance(stepId);

            // Act
            var result = app.AddNewStepInstance(stepId);

            // Assert
            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(MembershipApplicationErrors.StepAlreadyExists);
        }

        [Fact]
        public void Submit_ShouldLockPricingAndSetTimestamp()
        {
            // Arrange
            var app = MembershipApplication.Create(
                Guid.NewGuid(), "TRK-123", _userId, _clubId, Guid.NewGuid(), _planId, _templateId, 100.00m).Value;

            // Act
            var result = app.Submit();

            // Assert
            result.IsSuccess.Should().BeTrue();
            app.Status.Should().Be(ApplicationStatus.Submitted);
            app.PricingState.Should().Be(PricingState.Locked);
            app.SubmittedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Submit_ShouldFail_IfAlreadySubmitted()
        {
            // Arrange
            var app = MembershipApplication.Create(
                Guid.NewGuid(), "TRK-123", _userId, _clubId, Guid.NewGuid(), _planId, _templateId, 100.00m).Value;
            app.Submit();

            // Act
            var result = app.Submit();

            // Assert
            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(MembershipApplicationErrors.InvalidStatusTransition);
        }

        [Fact]
        public void ApplyPricing_ShouldReturnConflict_WhenPricingIsLocked()
        {
            // Arrange
            var app = MembershipApplication.Create(
                id: Guid.NewGuid(),
                trackingNumber: "TRK-" + Guid.NewGuid().ToString()[..8], // Generates a random short string
                userId: Guid.NewGuid(),
                clubId: Guid.NewGuid(),
                membershipTypeId: Guid.NewGuid(),
                membershipPlanId: Guid.NewGuid(),
                templateId: Guid.NewGuid(),
                basePrice: 100.00m
            ).Value;
            app.Submit(); // Locks pricing

            // Act
            var result = app.ApplyPricing(99.99m);

            // Assert
            result.IsError.Should().BeTrue();
            result.TopError.Type.Should().Be(ErrorKind.Conflict); // If your Error class has a Type property
            result.TopError.Code.Should().Be("MembershipApplication.PriceIsLocked");
        }
    }
}
