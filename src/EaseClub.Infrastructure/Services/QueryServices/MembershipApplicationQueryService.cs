using EaseClub.Application.Common.Pagination;
using EaseClub.Application.Features.MembershipApplications;
using EaseClub.Application.Features.MembershipApplications.Queries;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipApplications;
using EaseClub.Domain.MembershipApplications.Enums;
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


    public class MembershipApplicationQueryService : BaseQueryService<MembershipApplication>, IMembershipApplicationQueryService
    {
        public MembershipApplicationQueryService(AppDbContext context, ILogger<MembershipApplicationQueryService> logger) : base(context, logger)
        {
        }

        public async Task<Result<UnifiedPaginatedResponse<MembershipAppDto>>> GetMembershipApplicationSummaryAsync(Guid? clubId,
            Guid? userId,ApplicationStatus? status, PaginationRequest parameters, CancellationToken ct = default)
        {
            var query = Query();
            if (clubId != null)
            {
                query = query.Where(p => p.ClubId == clubId && p.UserId == userId);
            }
             if (userId != null)
            {
                query = query.Where(p => p.UserId == userId);
            }

            if (status != null)
            {
                query = query.Where(p => p.Status == status);
            }

            return await GetUnifiedPaginatedAsync<MembershipAppDto, DateTime>(
                query,
                parameters,
                selector: p => new MembershipAppDto()
                {
                    ClubId = p.ClubId,
                    UserId = p.UserId,
                    TrackingNumber = p.TrackingNumber,
                    MembershipType = p.MembershipType.Name,
                    MembershipPlanName = p.MembershipPlan.Name,
                    SubmittedAt = p.SubmittedAt,
                    Status = p.Status,

                },
                orderSelector: p => p.CreatedAt,
                ct
                );
        }
    }


}
