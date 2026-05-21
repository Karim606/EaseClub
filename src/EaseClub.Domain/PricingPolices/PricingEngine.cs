using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.PricingPolices
{
    public static class PricingEngine
    {
        public static PricingResult Calculate(decimal basePrice, IEnumerable<IPricingPolicy> policies, PricingContext context)
        {
            decimal totalAdjustments = 0;
            var applied = new List<AppliedPolicyDetail>();

            // We calculate based on the initial basePrice to ensure order-independence
            foreach (var policy in policies.OrderBy(p => p.Priority))
            {
                // Evaluate Conditions (Scenario 3, 4, 6)
                if (policy.Conditions != null && policy.Conditions.Any())
                {
                    if (!policy.Conditions.All(c => c.IsSatisfiedBy(context.Data.GetValueOrDefault(c.DependsOnFieldKey))))
                        continue;
                }

                decimal adjustment = 0;

                // Determine Base Adjustment (Scenario 1, 3)
                if (policy.PercentageValue.HasValue)
                    adjustment = basePrice * policy.PercentageValue.Value;
                else if (policy.FixedAmount.HasValue)
                    adjustment = policy.FixedAmount.Value;

                // Apply Multiplier (Scenario 2, 4)
                if (!string.IsNullOrEmpty(policy.MultiplierSourceKey))
                {
                    var val = context.Data.GetValueOrDefault(policy.MultiplierSourceKey);
                    decimal multiplier = decimal.TryParse(val, out var m) ? m : 0;
                    adjustment *= multiplier;
                }

                var finalAmount = policy.IsIncrease ? adjustment : -adjustment;
                totalAdjustments += finalAmount;
                applied.Add(new AppliedPolicyDetail(policy.Name, finalAmount));
            }

            return new PricingResult(basePrice, basePrice + totalAdjustments, applied);
        }

        /// <summary>
        /// Identifies all FieldKeys that a set of policies "watches".
        /// This tells the system which answers to pull into the PricingContext.
        /// </summary>
        public static HashSet<string> GetRequiredContextKeys(IEnumerable<IPricingPolicy> policies)
        {
            var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var policy in policies)
            {
                // Scenario 2, 4: Multiplier Key
                if (!string.IsNullOrEmpty(policy.MultiplierSourceKey))
                    keys.Add(policy.MultiplierSourceKey);

                // Scenario 3, 4, 6: Condition Keys
                if (policy.Conditions != null)
                {
                    foreach (var condition in policy.Conditions)
                    {
                        if (!string.IsNullOrEmpty(condition.DependsOnFieldKey))
                            keys.Add(condition.DependsOnFieldKey);
                    }
                }
            }

            return keys;
        }

    }
}
