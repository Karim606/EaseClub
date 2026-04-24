using Microsoft.Extensions.Logging;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Common.Pagination;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipTypes;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipTypes.Queries.GetMembershipsTypesForMember
{
    public class GetMembershipTypesForMemberQueryHandler(ICurrentUserService currentUserService,
        IMembershipTypesQueryService membershipTypeQueryService,
        ILogger<GetMembershipTypesForMemberQueryHandler> logger) : IRequestHandler<GetMembershipTypesForMemberQuery, Result<UnifiedPaginatedResponse<MembershipTypeDto>>>
    {
        public async Task<Result<UnifiedPaginatedResponse<MembershipTypeDto>>> Handle(GetMembershipTypesForMemberQuery request, CancellationToken cancellationToken)
        {

               
            var list =  await membershipTypeQueryService.GetMembershipTypesForUserAsync(request.ClubId,
                    request.BranchId,
                    request.accessToAllBranches,
                    true,
                    request.Pagination,
                    cancellationToken);
            return list;
        }
    }
}
