using EaseClub.Application.Common;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.PricingPolices;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.PricingPolicies.Commands.DeletePolicy
{
    public record DeletePricingPolicyCommand(Guid Id) : IRequest<Result<Success>>, IRequireClubOwnershipValidation
    {
        public IEnumerable<OwnershipRule> Rules()
        {
            yield return new OwnershipRule(
                (auth,clubId) => auth.DoesResourceBelongToClubAsync<PricingPolicy>(Id,clubId),
                nameof(PricingPolicies),
                Id);
        }
    }
}
