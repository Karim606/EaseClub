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
                DecisionsAboutApplication.Approved,
                null);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Decision.Should().Be(DecisionsAboutApplication.Approved);
            result.Value.Reason.Should().BeNull();
        }

        [Fact]
        public void Create_ShouldFail_WhenDecisionIsRejectAndReasonIsEmpty()
        {
            // Act
            var result = ApplicationReview.Create(
                Guid.NewGuid(),
                _appId,
                _reviewerId,
                DecisionsAboutApplication.Rejected,
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
                DecisionsAboutApplication.Rejected,
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
                DecisionsAboutApplication.Approved,
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
                DecisionsAboutApplication.Approved,
                null);

            // Assert
            // We check if the date is roughly "now" (within 1 second)
            result.Value.Date.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Create_ShouldFail_WhenDecisionIsRejectedAndNoteIsProvided()
        {
            // Act
            var result = ApplicationReview.Create(
                Guid.NewGuid(), _appId, _reviewerId,
                DecisionsAboutApplication.Rejected,
                "Some reason", "Some note");

            // Assert
            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(ApplicationReviewErrors.NoteNotAllowedForRejection);
        }

        [Fact]
        public void Create_ShouldFail_WhenDecisionIsNeedsCorrectionAndNoteIsEmpty()
        {
            // Act
            var result = ApplicationReview.Create(
                Guid.NewGuid(), _appId, _reviewerId,
                DecisionsAboutApplication.NeedsCorrection,
                null, " ");

            // Assert
            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(ApplicationReviewErrors.NeedsCorrectionNoteRequired);
        }

        [Fact]
        public void Create_ShouldSucceed_WhenDecisionIsNeedsCorrectionWithNote()
        {
            // Act
            var result = ApplicationReview.Create(
                Guid.NewGuid(), _appId, _reviewerId,
                DecisionsAboutApplication.NeedsCorrection,
                null, "Please upload your ID again.");

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Note.Should().Be("Please upload your ID again.");
        }
    }
}
