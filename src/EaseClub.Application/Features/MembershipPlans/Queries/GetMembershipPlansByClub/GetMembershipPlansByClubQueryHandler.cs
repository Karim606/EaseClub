using EaseClub.Application.Common.Pagination.Results;
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
    public class GetMembershipPlansByClubHandler(IMembershipPlanQueryService queryService)
    : IRequestHandler<GetMembershipPlansByClubQuery, Result<OffsetPaginatedResult<MembershipPlanDto>>>
    {
        public async Task<Result<OffsetPaginatedResult<MembershipPlanDto>>> Handle(
            GetMembershipPlansByClubQuery request,
            CancellationToken cancellationToken)
        {
            // We pass 'OffsetPaginatedResult' as the generic type TResult
            return await queryService.GetMembershipPlansByClubAsync<OffsetPaginatedResult<MembershipPlanDto>>(
                request.ClubId,
                request.MembershipTypeId,
                request.Parameters,
                cancellationToken);
        }
    }
}
