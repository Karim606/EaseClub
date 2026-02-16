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
    public class ApplicationReviewTests
    {
        private readonly Guid _appId = Guid.NewGuid();
        private readonly Guid _reviewerId = Guid.NewGuid();

        [Fact]
        public void Create_ShouldReturnSuccess_WhenDecisionIsApproveWithoutReason()
        {
            // Act
            var result = ApplicationReview.Create(
                Guid.NewGuid(),
                _appId,
                _reviewerId,
                DecisionsAboutApplication.Approve,
                null);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Decision.Should().Be(DecisionsAboutApplication.Approve);
            result.Value.Reason.Should().BeEmpty();
        }

        [Fact]
        public void Create_ShouldFail_WhenDecisionIsRejectAndReasonIsEmpty()
        {
            // Act
            var result = ApplicationReview.Create(
                Guid.NewGuid(),
                _appId,
                _reviewerId,
                DecisionsAboutApplication.Reject,
                " ");

            // Assert
            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(ApplicationReviewErrors.RejectionReasonRequired);
        }

        [Fact]
        public void Create_ShouldReturnSuccess_WhenDecisionIsRejectWithReason()
        {
            // Act
            var result = ApplicationReview.Create(
                Guid.NewGuid(),
                _appId,
                _reviewerId,
                DecisionsAboutApplication.Reject,
                "Credit score too low.");

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Reason.Should().Be("Credit score too low.");
        }

        [Fact]
        public void Create_ShouldFail_WhenReviewerIdIsEmpty()
        {
            // Act
            var result = ApplicationReview.Create(
                Guid.NewGuid(),
                _appId,
                Guid.Empty,
                DecisionsAboutApplication.Approve,
                null);

            // Assert
            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(ApplicationReviewErrors.ReviewerRequired);
        }

        [Fact]
        public void Create_ShouldSetCurrentDate()
        {
            // Act
            var result = ApplicationReview.Create(
                Guid.NewGuid(),
                _appId,
                _reviewerId,
                DecisionsAboutApplication.Approve,
                null);

            // Assert
            // We check if the date is roughly "now" (within 1 second)
            result.Value.Date.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }
    }
}
