using EaseClub.Application.Common.Pagination;
using EaseClub.Application.Features.Memberships.Queries;
using EaseClub.Application.Features.Memberships.Queries.GetMembershipsForAdmin;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Memberships;
using EaseClub.Infrastructure.Common.QueryServices;
using EaseClub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
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
            
            MembershipStatus? status,
            string search,
            PaginationRequest paginationRequest,
            CancellationToken cancellationToken = default)
        {
            var query = Query();
            query = query.Where(m => m.ClubId == clubId);

            query = query
            .Include(m => m.MembershipCycles)
            .Include(m => m.Member)
            .Include(m => m.MembershipType)
            .Include(m => m.MembershipPlan);

            if (status.HasValue)
            {
                query = query.Where(m => m.Status == status.Value);
            }
            if (!string.IsNullOrEmpty(search))
            {
                var normalized = search.ToLower();
                query = query.Where(m =>
                    m.Member.FirstName.ToLower().Contains(normalized) ||
                    m.Member.LastName.ToLower().Contains(normalized) ||
                    m.MembershipNumber.ToLower().Contains(normalized));
            }
            var now = DateTime.UtcNow;

            var pagedResult = await GetUnifiedPaginatedAsync(
                query,
                paginationRequest,
                selector: m => new MembershipsAdminDto(
                    m.Id,
                    m.Member.FirstName + " " + m.Member.LastName,
                    m.MembershipNumber,
                    m.MembershipType.Name,
                    m.MembershipPlan.Name,
                    m.MembershipPlan.MaxFamilyMembers > 0,

                     m.MembershipCycles
                    .Where(c => c.Period.StartDate <= now && c.Period.EndDate >= now)
                    .OrderBy(c => c.Period.StartDate)
                    .FirstOrDefault()
                    ?? 
                    m.MembershipCycles
                    .Where(c => c.Period.StartDate > now)
                    .OrderBy(c => c.Period.StartDate)
                    .FirstOrDefault(), 

                    m.CreatedAt,
                    m.Status
                   ),
                orderSelector: m => m.CreatedAt,
                cancellationToken);
            return pagedResult;
        }
    }
}
