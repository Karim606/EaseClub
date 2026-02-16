using EaseClub.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.MembershipApplications.Errors
{
    public static class PricingPolicyErrors
    {
        public static Error NameRequired =>
            Error.Validation("PricingPolicy.NameRequired",
                "Policy name is required.");

        public static Error ConditionRequired =>
            Error.Validation("PricingPolicy.ConditionRequired",
                "Condition expression is required.");

        public static Error InvalidEffectAmount =>
            Error.Validation("PricingPolicy.InvalidEffectAmount",
                "Effect amount must be greater than zero.");
    }
}
