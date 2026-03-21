using EaseClub.Application.Common.Pagination;
using EaseClub.Application.Features.MembershipTypes.Queries.GetMembershipTypesForAdmin;
using EaseClub.Domain.Common.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipTypes.Queries
{
    public interface IMembershipTypesQueryService
    {

        Task<Result<UnifiedPaginatedResponse<MembershipTypeDto>>>GetMembershipTypesForUserAsync(Guid?clubId,Guid?branchId,bool?allBranchesPermitted,bool?isActive,PaginationRequest paginationParameters,CancellationToken cancellationToken);

        Task<Result<UnifiedPaginatedResponse<MembershipTypeAdminDto>>> GetMembershipTypesForAdminAsync(Guid? clubId, Guid? branchId, bool? allBranchesPermitted, bool? isActive, PaginationRequest paginationParameters,
            CancellationToken cancellationToken);

    }
}
