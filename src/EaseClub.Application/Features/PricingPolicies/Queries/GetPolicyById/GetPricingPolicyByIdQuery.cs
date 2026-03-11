using EaseClub.Application.Common.Dtos;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.PricingPolicies.Queries.GetPolicyById
{
    public record GetPricingPolicyByIdQuery(Guid Id) : IRequest<Result<PricingPolicyResponse>>;

    // Response DTO
    public record PricingPolicyResponse(
        Guid Id,
        string Name,
        int Priority,
        bool IsIncrease,
        decimal? FixedAmount,
        decimal? PercentageValue,
        string? MultiplierSourceKey,
        List<ConditionDto> Conditions);


}
