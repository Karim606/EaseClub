using EaseClub.Application.Common.Pagination;
using EaseClub.Application.Features.Memberships.Queries.GetMembershipsForAdmin;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Memberships;
using EaseClub.Infrastructure.Common.QueryServices;
using EaseClub.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Services.QueryServices
{
    public class MembershipQueryService : BaseQueryService<Membership>
    {

        public MembershipQueryService(AppDbContext dbContext, ILogger<MembershipQueryService> logger) : base(dbContext, logger)
        {
        }

        public async Task<Result<UnifiedPaginatedResponse<MembershipsAdminDto>>> GetMembershipsForAdminAsync(Guid clubId, MembershipStatus? status,PaginationRequest paginationRequest, string search,CancellationToken ct)
        {
            var query = Query();
            if (status.HasValue)
            {
                query = query.Where(m => m.Status == status.Value);
            }
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(m => m.Member.FirstName.Contains(search) || m.Member.LastName.Contains(search) || m.MembershipNumber.Contains(search));
            }

            return await GetUnifiedPaginatedAsync<MembershipsAdminDto,string>(
                query,
                paginationRequest,
                m => new MembershipsAdminDto
                (
                    m.Id,
                   $"{m.Member.FirstName} {m.Member.LastName}",
                    m.MembershipNumber,
                    m.MembershipType.Name,
                    m.MembershipPlan.Name,
                    m.CreatedAt,
                    m.GetCurrentCycle().Period,
                    m.Status
                ),
                m => m.MembershipNumber,
                ct
            );
        }
    }
}
