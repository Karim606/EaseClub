using EaseClub.Application.Common.Pagination;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipApplications.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipApplications.Queries
{
    public interface IMembershipApplicationQueryService
    {
        Task<Result<UnifiedPaginatedResponse<MembershipAppDto>>> GetMembershipApplicationSummaryAsync(Guid? clubId, Guid? userId,
            ApplicationStatus? status,PaginationRequest parameters
            , CancellationToken ct = default);
    }
}
