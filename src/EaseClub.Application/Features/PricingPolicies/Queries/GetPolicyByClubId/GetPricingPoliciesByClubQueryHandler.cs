using Microsoft.Extensions.Logging;
using EaseClub.Application.Common.Dtos;
using EaseClub.Application.Features.PricingPolicies.Queries.GetPolicyById;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.PricingPolices;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.PricingPolicies.Queries.GetPolicyByClubId
{
    public class GetPricingPoliciesByClubQueryHandler(IPricingPolicyRepository policyRepository,
        ILogger<GetPricingPoliciesByClubQueryHandler> logger)
        : IRequestHandler<GetPricingPoliciesByClubQuery, Result<List<PricingPolicyResponse>>>
    {
        public async Task<Result<List<PricingPolicyResponse>>> Handle(GetPricingPoliciesByClubQuery request, CancellationToken ct)
        {
            return (await policyRepository.GetByClubIdAsync(request.ClubId, ct))
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
    }
}
