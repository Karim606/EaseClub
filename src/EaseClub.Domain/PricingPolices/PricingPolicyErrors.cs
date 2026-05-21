using EaseClub.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.PricingPolices
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

        public static Error CantCombinePercentageAndFixedAmount = Error.Validation(
            "PricingPolicy.CantCombinePercentageAndFixedAmount",
            "A policy cannot have both a fixed amount and a percentage value.");

        public static Error PercentageAndFixedAmountCantBothBeNull = Error.Validation(
            "PricingPolicy.PercentageAndFixedAmountCantBothBeNull",
            "A policy cannot have both a fixed amount and a percentage value.");
    }
}
