using EaseClub.Application.Common.Pagination;
using EaseClub.Application.Features.Memberships.Queries.GetMembershipsForAdmin;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Memberships;

namespace EaseClub.Application.Features.Memberships.Queries
{
    public interface IMembershipQueryService
    {
        Task<Result<UnifiedPaginatedResponse<MembershipsAdminDto>>> GetMembershipsForAdminAsync(
            Guid clubId,
            MembershipStatus? status,
            string search,
            PaginationRequest paginationRequest,
            CancellationToken cancellationToken = default);
    }
}
