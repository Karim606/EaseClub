using Microsoft.Extensions.Logging;
using EaseClub.Application.Common.Dtos;
using EaseClub.Application.Features.PricingPolicies.Queries.GetPolicyById;
using EaseClub.Domain.ApplicationTemplates.Repositories;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Events;
using EaseClub.Domain.PricingPolices;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.PricingPolicies.Queries.GetPolicyByClubId
{
    public class GetPricingPoliciesByClubQueryHandler(
        IPricingPolicyRepository policyRepository,
        IApplicationTemplateRepository templateRepository,
        ILogger<GetPricingPoliciesByClubQueryHandler> logger)
        : IRequestHandler<GetPricingPoliciesByClubQuery, Result<List<PricingPolicyResponse>>>
    {
        public async Task<Result<List<PricingPolicyResponse>>> Handle(GetPricingPoliciesByClubQuery request, CancellationToken ct)
        {
            var policies = await policyRepository.GetByClubIdAsync(request.ClubId, ct);

            // ── Compatibility filter ──────────────────────────────────────────
            if (request.CompatibleWithTarget.HasValue)
            {
                HashSet<string> allowedKeys;

                switch (request.CompatibleWithTarget.Value)
                {
                    case PricingPolicyTargetType.Event:
                        allowedKeys = EventFieldKeys.All;
                        break;

                    case PricingPolicyTargetType.ApplicationTemplate:
                        if (!request.CompatibleWithTargetId.HasValue)
                            return Error.Validation("Policy.MissingTargetId",
                                "CompatibleWithTargetId is required when filtering by ApplicationTemplate.");

                        var template = await templateRepository.GetTemplateWithStepsAsync(
                            request.CompatibleWithTargetId.Value, ct);
                        if (template is null)
                            return Error.NotFound("Template.NotFound");

                        allowedKeys = template.FieldKeys;
                        break;

                    default:
                        return Error.Validation("Policy.UnknownTargetType", "Unknown target type for compatibility filter.");
                }

                policies = policies.Where(p => IsPolicyCompatible(p, allowedKeys)).ToList();
            }
            // ─────────────────────────────────────────────────────────────────

            return policies
                .Select(p => new PricingPolicyResponse(
                    p.Id,
                    p.Name,
                    p.IsIncrease,
                    p.FixedAmount,
                    p.PercentageValue,
                    p.MultiplierSourceKey,
                    p.Conditions.Select(c => new ConditionDto(c.DependsOnFieldKey, c.Operator, c.ExpectedValue)).ToList()
                ))
                .ToList();
        }

        /// <summary>
        /// A policy is compatible with a target if every field key it references
        /// (in conditions AND as a multiplier source) is within the target's known key set.
        /// Policies with no conditions and no multiplier are always compatible.
        /// </summary>
        private static bool IsPolicyCompatible(PricingPolicy policy, HashSet<string> allowedKeys)
        {
            if (!string.IsNullOrEmpty(policy.MultiplierSourceKey) &&
                !allowedKeys.Contains(policy.MultiplierSourceKey))
                return false;

            return policy.Conditions
                .Where(c => !string.IsNullOrEmpty(c.DependsOnFieldKey))
                .All(c => allowedKeys.Contains(c.DependsOnFieldKey));
        }
    }
}

