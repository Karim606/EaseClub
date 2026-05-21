using Microsoft.Extensions.Logging;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Common.Pagination;
using EaseClub.Application.Features.MembershipPlans.Queries.GetMembershipPlansForAdmin;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipPlans.Queries.GetMembershipPlansForAdmin
{
    public class GetMembershipPlansForAdminHandler(IMembershipPlanQueryService membershipPlanQueryService,
        ILogger<GetMembershipPlansForAdminHandler> logger) : IRequestHandler<GetMembershipPlansForAdminQuery, Result<UnifiedPaginatedResponse<MembershipPlanAdminDto>>>
    {

        public async Task<Result<UnifiedPaginatedResponse<MembershipPlanAdminDto>>> Handle(
            GetMembershipPlansForAdminQuery request, CancellationToken cancellationToken)
        {

            var list = await membershipPlanQueryService.GetMembershipPlansForAdminAsync(
                request.ClubId,
                request.MembershipTypeId,
                request.IsActive,
                request.Pagination,
                cancellationToken
            );

            return list;
        }
    }
}
