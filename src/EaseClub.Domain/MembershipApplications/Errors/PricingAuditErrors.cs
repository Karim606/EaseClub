using EaseClub.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.MembershipApplications.Errors
{
    public static class PricingAuditErrors
    {
        public static Error ApplicationIdRequired =>
            Error.Validation("PricingAudit.ApplicationIdRequired",
                "ApplicationId is required.");

        public static Error PricingPolicyIdRequired =>
            Error.Validation("PricingAudit.PricingPolicyIdRequired",
                "PricingPolicyId is required.");

        public static Error InvalidOldPrice =>
            Error.Validation("PricingAudit.InvalidOldPrice",
                "Old price cannot be negative.");

        public static Error InvalidNewPrice =>
            Error.Validation("PricingAudit.InvalidNewPrice",
                "New price cannot be negative.");

        public static Error NoPriceChange =>
            Error.Validation("PricingAudit.NoPriceChange",
                "New price must be different from old price.");
    }
}
