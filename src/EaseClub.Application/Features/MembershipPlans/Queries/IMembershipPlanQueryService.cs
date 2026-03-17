using EaseClub.Application.Common.Pagination;
using EaseClub.Application.Common.Pagination.Parameters;
using EaseClub.Application.Common.Pagination.Results;
using EaseClub.Application.Features.MembershipPlans.Queries.GetMembershipPlansForMember;
using EaseClub.Application.Features.MembershipPlans.Queries.GetMembershipPlansForAdmin;
using EaseClub.Domain.Common.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipPlans.Queries
{
    public interface IMembershipPlanQueryService
    {
        Task<Result<UnifiedPaginatedResponse<MembershipPlanDto>>> GetMembershipPlansForMemberAsync(
            Guid? clubId,
            Guid? membershipTypeId,
            PaginationRequest parameters,
            CancellationToken ct);

        Task<Result<UnifiedPaginatedResponse<MembershipPlanAdminDto>>> GetMembershipPlansForAdminAsync(
            Guid? clubId,
            Guid? membershipTypeId,
            bool? isActive,
            PaginationRequest parameters,
            CancellationToken ct);
    }
}
