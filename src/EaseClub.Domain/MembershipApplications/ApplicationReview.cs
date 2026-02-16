using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipApplications.Enums;
using EaseClub.Domain.MembershipApplications.Errors;

namespace EaseClub.Domain.MembershipApplications
{
    public class ApplicationReview : AuditableEntity
    {
        private ApplicationReview() { }

        private ApplicationReview(
            Guid id,
            Guid applicationId,
            Guid reviewerId,
            DecisionsAboutApplication decision,
            string reason) : base(id)
        {
            ApplicationId = applicationId;
            ReviewerId = reviewerId;
            Decision = decision;
            Reason = reason;
            Date = DateTime.UtcNow;
        }

        public Guid ApplicationId { get; private set; }
        public Guid ReviewerId { get; private set; }

        public DecisionsAboutApplication Decision { get; private set; }
        public string Reason { get; private set; }
        public DateTime Date { get; private set; }

        #region Factory

        public static Result<ApplicationReview> Create(
            Guid id,
            Guid applicationId,
            Guid reviewerId,
            DecisionsAboutApplication decision,
            string? reason)
        {
            if (applicationId == Guid.Empty)
                return ApplicationReviewErrors.ApplicationRequired;

            if (reviewerId == Guid.Empty)
                return ApplicationReviewErrors.ReviewerRequired;

            if (!Enum.IsDefined(typeof(DecisionsAboutApplication), decision))
                return ApplicationReviewErrors.InvalidDecision;

            if (decision == DecisionsAboutApplication.Reject &&
                string.IsNullOrWhiteSpace(reason))
                return ApplicationReviewErrors.RejectionReasonRequired;

            var review = new ApplicationReview(
                id,
                applicationId,
                reviewerId,
                decision,
                reason ?? string.Empty);

            return review;
        }

        #endregion
    }
}