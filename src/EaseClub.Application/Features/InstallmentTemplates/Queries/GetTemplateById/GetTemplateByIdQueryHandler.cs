using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipPlans.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.InstallmentTemplates.Queries.GetTemplateById
{
    public class GetInstallmentTemplateQueryHandler(IInstallmentsTemplatesRepository repository)
    : IRequestHandler<GetInstallmentTemplateQuery, Result<InstallmentTemplateResponse>>
    {
    

        public async Task<Result<InstallmentTemplateResponse>> Handle(
            GetInstallmentTemplateQuery request,
            CancellationToken cancellationToken)
        {
            var template = await repository.GetByIdAsync(request.Id);

            if (template is null)
            {
                return Error.NotFound(description:"Installment template not found.");
            }

            return new InstallmentTemplateResponse(
                template.Id,
                template.Name,
                template.Installments.Select(i => new InstallmentDto(i.OrderIndex, i.PercentageOfAmount,i.DueAfterDays)).ToList()
            );
        }

    }
   
}
