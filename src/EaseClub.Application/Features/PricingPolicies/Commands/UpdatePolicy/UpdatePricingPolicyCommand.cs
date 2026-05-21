using EaseClub.Application.Common;
using EaseClub.Application.Common.Dtos;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.PricingPolices;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.PricingPolicies.Commands.UpdatePolicy
{
    public record UpdatePricingPolicyCommand(
    Guid Id,
    string Name,
    int Priority,
    bool IsIncrease,
    decimal? FixedAmount,
    decimal? PercentageValue,
    string? MultiplierKey,
    List<ConditionDto> Conditions) : IRequest<Result<Success>>, IRequireClubOwnershipValidation
    {
        public IEnumerable<OwnershipRule> Rules()
        {
            yield return new OwnershipRule(
                async(auth, clubId) => await auth.DoesResourceBelongToClubAsync<PricingPolicy>(Id,clubId),
                nameof(UpdatePricingPolicyCommand),
                Id
                );
        }
    }
}
