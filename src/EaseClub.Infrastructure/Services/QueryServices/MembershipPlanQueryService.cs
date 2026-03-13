using EaseClub.Application.Common.Pagination.Parameters;
using EaseClub.Application.Common.Pagination.Results;
using EaseClub.Application.Features.MembershipPlans.Queries;
using EaseClub.Application.Features.MembershipPlans.Queries.GetMembershipPlansByClub;

using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipPlans;

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
    public class MembershipPlanQueryService
    : BaseQueryService<MembershipPlan>, IMembershipPlanQueryService
    {
        public MembershipPlanQueryService(
            AppDbContext context,
            ILogger<MembershipPlanQueryService> logger) : base(context, logger)
        {
        }

        public async Task<Result<TResult>> GetMembershipPlansByClubAsync<TResult>(
            Guid clubId,
            Guid? membershipTypeId,
            PaginationParameters parameters,
            CancellationToken ct) where TResult : PaginatedResult<MembershipPlanDto>, new()
        {
            // 1. Build the base filter
            var query = Query().Where(p => p.ClubId == clubId);

            if(membershipTypeId.HasValue) query = query.Where(p => p.MembershipTypeId == membershipTypeId.Value);
            // 2. Optional: Add search if your parameters include it
            // query = query.ApplySearch(parameters.Search, p => p.Name);

            // 3. Delegate the heavy lifting to the Base class
            return await GetPaginatedAsync<MembershipPlanDto, string, TResult>(
                query,
                parameters,
                selector: p => new MembershipPlanDto(
                    p.Id,
                    p.Name,
                    p.MaxPaymentPeriodInDays,
                    p.TotalPrice,
                    p.Description),
                orderSelector: p => p.Name, // Default sorting by Name
                cancellationToken: ct
            );
        }
    }
}
