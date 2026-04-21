using EaseClub.Application.Common.Pagination;
using EaseClub.Application.Features.Memberships.Queries;
using EaseClub.Domain.Common.Results;
using MediatR;

namespace EaseClub.Application.Features.Memberships.Queries.GetMembershipsForAdmin
{
    public class GetMembershipsForAdminQueryHandler(IMembershipQueryService membershipQueryService)
        : IRequestHandler<GetMembershipsForAdminQuery, Result<UnifiedPaginatedResponse<MembershipsAdminDto>>>
    {
        public async Task<Result<UnifiedPaginatedResponse<MembershipsAdminDto>>> Handle(GetMembershipsForAdminQuery request, CancellationToken cancellationToken)
        {
            return await membershipQueryService.GetMembershipsForAdminAsync(
                request.ClubId,
                request.Status,
                request.Search,
                request.PaginationRequest,
                cancellationToken);
        }
    }
}
