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
    IInstallmentsTemplatesRepository repository) // New Query Service
    : IRequestHandler<GetInstallmentTemplatesByClubQuery, Result<List<TemplatesResponse>>>
    {
        public async Task<Result<List<TemplatesResponse>>> Handle(
            GetInstallmentTemplatesByClubQuery request,
            CancellationToken ct)
        {

            var list = await repository.GetTemplatesAsync(request.ClubId, request.PlanId, true,ct);

            return list.Select(i => new TemplatesResponse(i.Id,i.Name,i.Installments.Count,i.Installments.Max(i => i.DueAfterDays))).ToList();

        }
    }
}
