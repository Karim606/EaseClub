using EaseClub.Application.Features.PricingPolicies.Queries.GetPolicyById;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.PricingPolicies.Queries.GetPolicyByClubId
{
    public record GetPricingPoliciesByClubQuery(Guid ClubId) : IRequest<Result<List<PricingPolicyResponse>>>;
}
