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
    /// <param name="CompatibleWithTarget">
    /// Optional filter. When set, only returns policies whose field keys are all
    /// contained in the target's known field keys:
    /// - Event → checks against EventFieldKeys.All
    /// - ApplicationTemplate → requires CompatibleWithTargetId to load that template's field keys
    /// </param>
    public record GetPricingPoliciesByClubQuery(
        Guid ClubId,
        PricingPolicyTargetType? CompatibleWithTarget = null,
        Guid? CompatibleWithTargetId = null) : IRequest<Result<List<PricingPolicyResponse>>>;
}
