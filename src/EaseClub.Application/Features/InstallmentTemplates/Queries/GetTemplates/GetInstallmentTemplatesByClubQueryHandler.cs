using EaseClub.Application.Common.Pagination;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipPlans;
using EaseClub.Domain.MembershipPlans.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.InstallmentTemplates.Queries.GetTemplates
{
    public class GetInstallmentTemplatesByClubQueryHandler(
    IInstallmentsTemplatesRepository repository,
    IInstallmentTemplateQueryService queryService) // New Query Service
    : IRequestHandler<GetInstallmentTemplatesByClubQuery, Result<UnifiedPaginatedResponse<TemplatesResponse>>>
    {
        public async Task<Result<UnifiedPaginatedResponse<TemplatesResponse>>> Handle(
            GetInstallmentTemplatesByClubQuery request,
            CancellationToken ct)
        {

            return await queryService.GetTemplatesByClubAsync(
            request.ClubId,
            request.Parameters,
            ct);
        }
    }
}
