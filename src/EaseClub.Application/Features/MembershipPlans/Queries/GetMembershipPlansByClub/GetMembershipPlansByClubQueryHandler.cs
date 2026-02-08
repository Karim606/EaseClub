using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipPlans.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipPlans.Queries.GetMembershipPlansByClub
{
    public class GetMembershipPlansByClubHandler(IMembershipPlanRepository membershipPlanRepository) 
        : IRequestHandler<GetMembershipPlansByClubQuery, Result<List<MembershipPlanDto>>>
    {


        public async Task<Result<List<MembershipPlanDto>>> Handle(GetMembershipPlansByClubQuery request, CancellationToken cancellationToken)
        {
            var plans = await membershipPlanRepository.GetPlansByClubAsync(request.ClubId, cancellationToken);

            return plans
            .Select(p => new MembershipPlanDto(p.Id, p.Name,p.DurationInDays, p.TotalPrice, p.Description))
            .ToList();
        }
    }
}
