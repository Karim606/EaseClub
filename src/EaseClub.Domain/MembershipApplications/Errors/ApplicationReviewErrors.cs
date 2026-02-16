using EaseClub.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.MembershipApplications.Errors
{
    public static class ApplicationReviewErrors
    {
        public static Error ApplicationRequired =>
            Error.Validation("ApplicationReview.ApplicationRequired",
                "ApplicationId is required.");

        public static Error ReviewerRequired =>
            Error.Validation("ApplicationReview.ReviewerRequired",
                "ReviewerId is required.");

        public static Error InvalidDecision =>
            Error.Validation("ApplicationReview.InvalidDecision",
                "Decision value is invalid.");

        public static Error RejectionReasonRequired =>
            Error.Validation("ApplicationReview.RejectionReasonRequired",
                "Reason is required when application is rejected.");
    }
}
