using EaseClub.Application.Common.Pagination.Parameters;
using EaseClub.Application.Common.Pagination.Results;
using EaseClub.Application.Features.MembershipPlans.Queries.GetMembershipPlansByClub;
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
        Task<Result<TResult>> GetMembershipPlansByClubAsync<TResult>(
            Guid clubId,
            PaginationParameters parameters,
            CancellationToken ct) where TResult : PaginatedResult<MembershipPlanDto>, new();
    }
}
