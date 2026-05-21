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
            string? reason,
            string? note) : base(id)
        {
            ApplicationId = applicationId;
            ReviewerId = reviewerId;
            Decision = decision;
            Reason = reason;
            Date = DateTime.UtcNow;
            Note = note;
        }

        public Guid ApplicationId { get; private set; }
        public Guid ReviewerId { get; private set; }

        public DecisionsAboutApplication Decision { get; private set; }
        public string? Reason { get; private set; }
        public string? Note { get; private set; }
        public DateTime Date { get; private set; }

        #region Factory

        public static Result<ApplicationReview> Create(
            Guid id,
            Guid applicationId,
            Guid reviewerId,
            DecisionsAboutApplication decision,
            string? reason=null,
            string? note = null)
        {
            if (applicationId == Guid.Empty)
                return ApplicationReviewErrors.ApplicationRequired;

            if (reviewerId == Guid.Empty)
                return ApplicationReviewErrors.ReviewerRequired;

            if (!Enum.IsDefined(typeof(DecisionsAboutApplication), decision))
                return ApplicationReviewErrors.InvalidDecision;

            if (decision == DecisionsAboutApplication.Rejected &&
                string.IsNullOrWhiteSpace(reason))
                return ApplicationReviewErrors.RejectionReasonRequired;

            if (decision == DecisionsAboutApplication.Rejected && !string.IsNullOrEmpty(note))
                return ApplicationReviewErrors.NoteNotAllowedForRejection;

            if (decision == DecisionsAboutApplication.NeedsCorrection && string.IsNullOrWhiteSpace(note))
                return ApplicationReviewErrors.NeedsCorrectionNoteRequired;

            var review = new ApplicationReview(
                id,
                applicationId,
                reviewerId,
                decision,
                reason,
                note);

            return review;
        }

        #endregion
    }
}