using EaseClub.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.MembershipApplications.Errors
{
    public static class MembershipApplicationErrors
    {
        public static Error TrackingNumberRequired =>
            Error.Validation("MembershipApplication.TrackingNumberRequired",
                "Tracking number is required.");

        public static Error UserIdRequired =>
            Error.Validation("MembershipApplication.UserIdRequired",
                "UserId is required.");

        public static Error ClubIdRequired =>
            Error.Validation("MembershipApplication.ClubIdRequired",
                "ClubId is required.");

        public static Error MembershipTypeRequired =>
            Error.Validation("MembershipApplication.MembershipTypeRequired",
                "MembershipTypeId is required.");

        public static Error MembershipPlanRequired =>
            Error.Validation("MembershipApplication.MembershipPlanRequired",
                "MembershipPlanId is required.");

        public static Error InvalidBasePrice =>
            Error.Validation("MembershipApplication.InvalidBasePrice",
                "Base price cannot be negative.");

        public static Error StepAlreadyExists =>
            Error.Conflict("MembershipApplication.StepAlreadyExists",
                "This step instance already exists.");

        public static Error InvalidStatusTransition =>
            Error.Validation("MembershipApplication.InvalidStatusTransition",
                "Invalid status transition.");

        public static Error CannotSubmitWithoutSteps =>
            Error.Validation("MembershipApplication.CannotSubmitWithoutSteps",
                "Application cannot be submitted without steps.");

        public static Error ApplicationLocked =>
            Error.Conflict("MembershipApplication.ApplicationLocked",
                "Application is locked and cannot be modified.");

        public static Error CantModifyNonDraft => Error.Conflict(
            code: "MembershipApplication.Cant.Modify.Non.Draft.Application",
            description: "Cant Modify Non-Draft  Application");

        public static Error PricingLocked =>
        Error.Conflict("MembershipApplication.PriceIsLocked",
            "The pricing for this application has been locked and cannot be recalculated.");
    }
}
