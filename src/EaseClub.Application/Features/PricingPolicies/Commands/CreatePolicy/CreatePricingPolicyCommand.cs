using EaseClub.Application.Common.Dtos;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.PricingPolicies.Commands.CreatePolicy
{
    public record CreatePricingPolicyCommand(
    Guid ClubId,
    string Name,
    bool IsIncrease,
    decimal? FixedAmount,
    decimal? PercentageValue,
    string? MultiplierKey,
    List<ConditionDto>? Conditions) : IRequest<Result<Guid>>, IRequireClubAdmin;

}
