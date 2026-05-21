using EaseClub.Application.Common.Pagination.Parameters;
using EaseClub.Application.Common.Pagination.Results;
using EaseClub.Application.Features.ApplicationTemplates.Commands;
using EaseClub.Application.Features.ApplicationTemplates.Queries;
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
            where TResult : PaginatedResult<TemplateSummaryDto>, new()
        {
            var query = Query().Where(at => at.ClubId == clubId);

            var result = await GetPaginatedAsync<TemplateSummaryDto, string, TResult>(
                 query,
                parameters,
                selector: at => new TemplateSummaryDto(at.Id, at.Name,at.IsActive, at.CreatedAt,at.UpdatedAt, at.ConnectedMembershipPlans.Select(t=>t.Name).ToList()),
                orderSelector: at => at.Name,
                ct
                );

            return result;
        }


        public async Task<Result<StepQuery>> GetTemplateStepByOrder(Guid templateId,int order,CancellationToken ct)
        {
            var template = await _context.ApplicationTemplateDefinitions
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == templateId, ct);

            if (template == null)
                return Error.NotFound("Template.NotFound", "The application template was not found.");

            var step = template.Steps.FirstOrDefault(s => s.Order == order);
            
            if (step == null)
                return Error.NotFound("TemplateStep.NotFound", $"Step {order} not found for this template.");

            return new StepQuery(
                step.Id,
                step.Title,
                step.Order,
                step.Sections.OrderBy(sec => sec.Order).Select(sec => new SectionQuery(
                    sec.Id,
                    sec.Title,
                    sec.Order,
                    sec.Intent,
                    sec.RepeatRule,
                    sec.Fields.OrderBy(f => f.Order).Select(f => new FieldQuery(
                        f.Id,
                        f.Key,
                        f.Label,
                        f.FieldType,
                        f.ValidationRules.ToDomain(), 
                        f.AllowedValues,
                        f.IsSystemField
                    )).ToList()
                )).ToList()
            );
        }

        public async Task<Result<TemplateTreeQuery>> GetFullTemplateTreeAsync(Guid templateId,CancellationToken ct) {

            var template = await _context.ApplicationTemplateDefinitions
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == templateId, ct);

            if (template == null) return Error.NotFound("Template.NotFound");

            return new TemplateTreeQuery(
                     template.Id,
                     template.Name,
                     template.Steps.OrderBy(s => s.Order).Select(s => new StepQuery(
                        s.Id,
                        s.Title,
                        s.Order,
                        s.Sections.OrderBy(sec => sec.Order).Select(sec => new SectionQuery (
                            sec.Id,
                            sec.Title,
                            sec.Order,
                            sec.Intent,
                            sec.RepeatRule,
                            sec.Fields.OrderBy(f => f.Order).Select(f => new FieldQuery (
                              f.Id,
                              f.Key,
                              f.Label,
                              f.FieldType,
                              f.ValidationRules.ToDomain(),
                              f.AllowedValues,
                              f.IsSystemField
                             )).ToList()
                        )).ToList()
                    )).ToList()
            );
        }
    }

}
