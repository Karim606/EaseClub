using EaseClub.Application.Common.Pagination;
using EaseClub.Application.Common.Pagination.Parameters;
using EaseClub.Application.Features.InstallmentTemplates.Queries.GetTemplates;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipPlans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.InstallmentTemplates.Queries
{
    public interface IInstallmentTemplateQueryService
    {
        public Task<Result<UnifiedPaginatedResponse<TemplatesResponse>>> GetTemplatesByClubAsync(
       Guid clubId,
       PaginationRequest parameters,
       CancellationToken ct);
    }
}
