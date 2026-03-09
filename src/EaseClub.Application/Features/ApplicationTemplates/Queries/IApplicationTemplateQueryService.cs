using EaseClub.Application.Common.Pagination.Parameters;
using EaseClub.Application.Common.Pagination.Results;
using EaseClub.Application.Features.ApplicationTemplates.Queries.GetTemplates;
using EaseClub.Domain.Common.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.ApplicationTemplates.Queries
{
    public interface IApplicationTemplateQueryService
    {
        public Task<Result<TResult>> GetTemplatesByClubAsync<TResult>(Guid clubId,PaginationParameters parameters,
            CancellationToken ct)
            where TResult : PaginatedResult<TemplateSummaryDto>, new();

        public Task<Result<StepDetailsDto>> GetTemplateStepByOrder(Guid templateId, int order,
            CancellationToken ct);

        public Task<Result<TemplateDetailsDto>> GetFullTemplateTreeAsync(Guid templateId, CancellationToken ct);
    }
}
