using EaseClub.Domain.ApplicationTemplates.ValueObjects.ConditionExpression;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.PricingPolices
{
    public class PricingPolicyAssignment : AuditableEntity
    {
        private PricingPolicyAssignment() { }

        private PricingPolicyAssignment(
            Guid id,
            Guid policyId,
            Guid targetId,
            int priority,
            PricingPolicyTargetType targetType) : base(id)
        {
            PolicyId = policyId;
            TargetId = targetId;
            TargetType = targetType;
            Priority = priority;
        }

        public Guid ClubId { get; private set; }
        public Guid PolicyId { get; private set; }
        public Guid TargetId { get; private set; }
        public int Priority { get; private set; }

        // String not enum — stays extensible without migrations
        public PricingPolicyTargetType TargetType { get; private set; }


        public static Result<PricingPolicyAssignment> Create(
            Guid policyId,
            Guid targetId,
            int priority,
            PricingPolicyTargetType targetType,
            PricingPolicy policy,
            HashSet<string> targetFieldKeys)
        {
            if(priority <=0) return Error.Validation("Policy.InvalidPriority", "Priority must be greater than 0.");
            // Validate at assignment time — not at runtime
            var requiredKeys = policy.Conditions
                .Select(c => c.DependsOnFieldKey)
                .Where(k => !string.IsNullOrEmpty(k))
                .ToList();

            if (policy.MultiplierSourceKey != null)
                requiredKeys.Add(policy.MultiplierSourceKey);

            var missingKeys = requiredKeys
                .Where(k => !targetFieldKeys.Contains(k))
                .ToList();

            if (missingKeys.Any())
                return Error.Validation(
                    "Policy.MissingKeys",
                    $"Target is missing required field keys: {string.Join(", ", missingKeys)}");

            return new PricingPolicyAssignment(
                Guid.NewGuid(),
                policyId,
                targetId,
                priority,
                targetType);
        }
    }
}
