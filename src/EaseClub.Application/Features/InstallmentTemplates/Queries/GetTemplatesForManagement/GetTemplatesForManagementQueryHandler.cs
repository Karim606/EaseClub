using Microsoft.Extensions.Logging;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipPlans.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.InstallmentTemplates.Queries.GetTemplatesForManagement
{
    public class GetTemplatesForManagementQueryHandler(IInstallmentsTemplatesRepository installmentsTemplatesRepository,
        ILogger<GetTemplatesForManagementQueryHandler> logger) : IRequestHandler<GetTemplatesForManagementQuery, Result<List<InstallmentTemplateAdminsDto>>>
    {
        public async Task<Result<List<InstallmentTemplateAdminsDto>>> Handle(GetTemplatesForManagementQuery request, CancellationToken cancellationToken)
        {
            var list = await installmentsTemplatesRepository.GetTemplatesAsync(request.clubId, request.planId, request.isActive, cancellationToken);
            return list.Select(i => new InstallmentTemplateAdminsDto
            {
                Id = i.Id,
                Name = i.Name,
                ClubId = i.ClubId,
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt,
                IsActive = i.IsActive,
                DurationOfPaymentInDays = i.Installments.Max(x => x.DueAfterDays),
                numOfInstallments = i.Installments.Count

            }).ToList();
        }
    }
}
