using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Common.Pagination;
using EaseClub.Application.Common.Pagination.Results;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipPlans.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipPlans.Queries.GetMembershipPlansForMember
{
    public class GetMembershipPlansForMemberQueryHandler(IMembershipPlanRepository membershipPlanRepository,
        IMembershipPlanQueryService membershipPlanQueryService,
        IClubAuthorizationService clubAuthorizationService)
    : IRequestHandler<GetMembershipPlansForMemberQuery, Result<UnifiedPaginatedResponse<MembershipPlanDto>>>
    {
        public async Task<Result<UnifiedPaginatedResponse<MembershipPlanDto>>> Handle(
            GetMembershipPlansForMemberQuery request,
            CancellationToken cancellationToken)
        {
            var list = await membershipPlanQueryService.GetMembershipPlansForMemberAsync(
             request.ClubId,
             request.MembershipTypeId,
             request.pagination,
             cancellationToken
         );

            return list;
        }
    }
}
