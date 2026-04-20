using EaseClub.Application.Common.Pagination;
using EaseClub.Application.Features.Memberships.Queries.GetMembershipsForAdmin;
using EaseClub.Domain.Common.Results;

namespace EaseClub.Application.Features.Memberships.Queries
{
    public interface IMembershipQueryService
    {
        Task<Result<UnifiedPaginatedResponse<MembershipsAdminDto>>> GetMembershipsForAdminAsync(
            Guid clubId,
            PaginationRequest paginationRequest,
            CancellationToken cancellationToken = default);
    }
}
