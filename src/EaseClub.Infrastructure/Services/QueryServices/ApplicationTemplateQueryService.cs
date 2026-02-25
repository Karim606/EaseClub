using EaseClub.Application.Common.Pagination.Parameters;
using EaseClub.Application.Common.Pagination.Results;
using EaseClub.Application.Features.ApplicationTemplates.Queries;
using EaseClub.Application.Features.ApplicationTemplates.Queries.GetStepByOrder;
using EaseClub.Application.Features.ApplicationTemplates.Queries.GetTemplates;
using EaseClub.Domain.ApplicationTemplates;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Infrastructure.Common.QueryServices;
using EaseClub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Services.QueryServices
{


    public class ApplicationTemplateQueryService : BaseQueryService<ApplicationTemplateDefinition>, IApplicationTemplateQueryService
    {
        public ApplicationTemplateQueryService(AppDbContext context, ILogger<ApplicationTemplateQueryService> logger) : base(context, logger)
        {
        }

        public async Task<Result<TResult>> GetTemplatesByClubAsync<TResult>(Guid clubId,
            PaginationParameters parameters,
            CancellationToken ct
            )
            where TResult : PaginatedResult<ApplicationTemplateDto>, new()
        {
            var query = Query().Where(at => at.ClubId == clubId);

            var result = await GetPaginatedAsync<ApplicationTemplateDto, string, TResult>(
                 query,
                parameters,
                selector: at => new ApplicationTemplateDto(at.ClubId, at.Name, at.IsActive, at.Steps.Count),
                orderSelector: at => at.Name,
                ct
                );

            return result;
        }


        public async Task<Result<StepDetailsDto>> GetTemplateStepByOrder(Guid templateId,int order,CancellationToken ct)
        {
            var step = await   _context.ApplicationStepDefinitions
                .Include(s => s.Sections)
                .ThenInclude(sec => sec.Fields)
                .Where(s => s.TemplateId == templateId && s.Order == order)
                .FirstOrDefaultAsync(ct);

            if (step == null)
                return Error.NotFound("TemplateStep.NotFound", $"Step {order} not found for this template.");

            // 2. Map to DTO
            var dto = new StepDetailsDto(
                step.Id,
                step.Title,
                step.Category,
                step.Order,
                step.Sections.OrderBy(sec => sec.Order).Select(sec => new SectionDetailsDto(
                    sec.Id,
                    sec.Title,
                    sec.IsRepeatable,
                    sec.Order,
                    sec.Fields.OrderBy(f => f.Order).Select(f => new FieldDetailsDto(
                        f.Id,
                        f.Key,
                        f.Type,
                        f.ValidationRules.IsRequired,
                        f.ValidationRules,
                        f.VisibilityCondition
                    )).ToList(),
                    sec.RepeatRule
                )).ToList()
            );

            return dto;
        }

        public async Task<Result<TemplateDetailsDto>> GetFullTemplateTreeAsync(Guid templateId,CancellationToken ct) {

            var templateData = await _context.ApplicationTemplateDefinitions
            .AsNoTracking()
            .AsSplitQuery() // <--- Add this here
            .Where(t => t.Id == templateId)
            .Select(t => new TemplateDetailsDto(
                     t.Id,
                     t.Name,
                     t.Steps.OrderBy(s => s.Order).Select(s => new StepDetailsDto(
                        s.Id,
                        s.Title,
                        s.Category,
                        s.Order,
                        s.Sections.OrderBy(sec => sec.Order).Select(sec => new SectionDetailsDto (
                            sec.Id,
                            sec.Title,
                            sec.IsRepeatable,
                            sec.Order,
                            sec.Fields.OrderBy(f => f.Order).Select(f => new FieldDetailsDto (
                              f.Id,
                              f.Key,
                              f.Type,
                              f.ValidationRules.IsRequired,
                              f.ValidationRules,
                              f.VisibilityCondition
                             )).ToList(),
                            sec.RepeatRule
                        )).ToList()
                    )).ToList()
            )).FirstOrDefaultAsync(ct);

            return templateData;
        }
    }

}
