using EaseClub.Application.Common;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipApplications;
using EaseClub.Domain.MembershipPlans;
using EaseClub.Domain.PricingPolices;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipApplications.Queries.GetPricingForApplication
{
    public record GetApplicationPricingQuery(Guid ApplicationId)
    : IRequest<Result<Pricing>>, IRequireClubOwnershipValidation
    {
        public IEnumerable<OwnershipRule> Rules()
        {
            yield return new OwnershipRule(
                async (auth, clubId) =>
                  ( (await auth.DoesResourceBelongToClubAsync<MembershipApplication>(ApplicationId, clubId)) || (await auth.DoesResourceBelongToCurrentUserAsync<MembershipApplication>(ApplicationId)) ),
                nameof(MembershipApplication),
                ApplicationId
            );
        }
    };

    public record Pricing(PricingResult PricingResult,List<InstallmentBlueprint> InstallmentBlueprints);
}
