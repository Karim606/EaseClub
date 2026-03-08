using EaseClub.Application.Common.Pagination;
using EaseClub.Application.Common.Pagination.Parameters;
using EaseClub.Application.Common.Pagination.Results;
using EaseClub.Application.Features.InstallmentTemplates.Queries;
using EaseClub.Application.Features.InstallmentTemplates.Queries.GetTemplates;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipPlans;
using EaseClub.Infrastructure.Common.QueryServices;
using EaseClub.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Services.QueryServices
{
    public class InstallmentTemplateQueryService : BaseQueryService<InstallmentTemplate>, IInstallmentTemplateQueryService
    {
        public InstallmentTemplateQueryService(AppDbContext context, ILogger<InstallmentTemplateQueryService> logger)
            : base(context, logger)
        {
        }

        public async Task<Result<UnifiedPaginatedResponse<TemplatesResponse>>> GetTemplatesByClubAsync(
        Guid clubId,
        PaginationRequest parameters,
        CancellationToken ct)
        {
            // 1. Build the base query
            var query = Query().Where(at => at.ClubId == clubId);

            // 2. Call the base pagination (passing a concrete type to satisfy the 'new()' constraint)
            var result = await GetUnifiedPaginatedAsync<TemplatesResponse, string>(
                query,
                parameters,
                selector: t => new TemplatesResponse(
                    t.Id,
                    t.Name,
                    t.Installments.Count,
                    t.Installments.Any() ? t.Installments.Max(i => i.DueAfterDays) : 0),
                orderSelector: t => t.Name,
                ct);

            if (result.IsError) return result.TopError;

            // 3. Map to the Unified Envelope (Use a helper to keep it clean)
            return result;
        }
    }
}
