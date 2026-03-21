using EaseClub.Application.Common.Pagination;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipTypes.Queries.GetMembershipTypesForAdmin
{
    public class GetMembershipTypesForAdminQueryHandler(IMembershipTypesQueryService membershipTypesQueryService) :
       IRequestHandler<GetMembershipTypesForAdminQuery, Result<UnifiedPaginatedResponse<MembershipTypeAdminDto>>>
    {

        public async Task<Result<UnifiedPaginatedResponse<MembershipTypeAdminDto>>> Handle(
            GetMembershipTypesForAdminQuery request,
            CancellationToken cancellationToken)
        {

            var result = await membershipTypesQueryService.GetMembershipTypesForAdminAsync(
                request.ClubId,
                request.BranchId,
                request.AccessToAllBranches,
                request.IsActive,
                request.Pagination,
                cancellationToken
            );

            return result;
        }
    }
}
