using EaseClub.Application.Common.Dtos;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.PricingPolices;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.PricingPolicies.Queries.GetPolicyById
{
    public class GetPricingPolicyByIdQueryHandler(
        IPricingPolicyRepository policyRepository) : IRequestHandler<GetPricingPolicyByIdQuery, Result<PricingPolicyResponse>>
    {
        public async Task<Result<PricingPolicyResponse>> Handle(GetPricingPolicyByIdQuery request, CancellationToken ct)
        {
            var policy = await policyRepository.GetByIdAsync(request.Id);

            if (policy == null)
                return Error.NotFound(description: $"Pricing policy with ID {request.Id} was not found.");

            // Mapping domain to DTO
            return new PricingPolicyResponse(
                policy.Id,
                policy.Name,
                policy.Priority,
                policy.IsIncrease,
                policy.FixedAmount,
                policy.PercentageValue,
                policy.MultiplierSourceKey,
                policy.Conditions.Select(c => new ConditionDto(c.DependsOnFieldKey, c.Operator, c.ExpectedValue)).ToList()
            );
        }
    }
}
