using EaseClub.Application.Common.Pagination;
using EaseClub.Application.Features.Memberships.Queries;
using EaseClub.Application.Features.Memberships.Queries.GetMembershipsForAdmin;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Memberships;
using EaseClub.Infrastructure.Common.QueryServices;
using EaseClub.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace EaseClub.Infrastructure.Services.QueryServices
{
    public class MembershipQueryService : BaseQueryService<Membership>, IMembershipQueryService
    {
        public MembershipQueryService(AppDbContext context, ILogger<MembershipQueryService> logger)
            : base(context, logger)
        {
        }

        public async Task<Result<UnifiedPaginatedResponse<MembershipsAdminDto>>> GetMembershipsForAdminAsync(
            Guid clubId,
            PaginationRequest paginationRequest,
            CancellationToken cancellationToken = default)
        {
            var query = Query();

            return await GetUnifiedPaginatedAsync(
                query,
                paginationRequest,
                selector: m => new MembershipsAdminDto(
                    m.Id,
                    m.Member.FirstName + " " + m.Member.LastName,
                    m.MembershipNumber,
                    m.MembershipType.Name,
                    m.MembershipPlan.Name,
                    m.CreatedAt,
                    m.Status),
                orderSelector: m => m.CreatedAt,
                cancellationToken);
        }
    }
}
