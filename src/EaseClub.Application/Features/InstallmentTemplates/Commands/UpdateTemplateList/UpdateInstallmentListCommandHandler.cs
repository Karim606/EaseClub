using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipPlans;
using EaseClub.Domain.MembershipPlans.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.InstallmentTemplates.Commands.UpdateTemplateList
{
   
        public class UpdateInstallmentListCommandHandler(IInstallmentsTemplatesRepository repository,
        IUnitOfWork unitOfWork) : IRequestHandler<UpdateInstallmentListCommand, Result<Success>>
        {
           
            public async Task<Result<Success>> Handle(UpdateInstallmentListCommand request, CancellationToken ct)
            {
                var template = await repository.GetByIdAsync(request.TemplateId, ct);
                if (template is null) return Error.NotFound("Template not found.");

                List<Installment> installments = new List<Installment>();

                foreach (var installment in request.Installments)
                {
                    var res = Installment.Create(installment.Percentage, installment.DueAfterDays, installment.Order);
                    if (res.IsError) return res.TopError;
                    installments.Add(res.Value);
                }
                // Domain method to sync the list (reconciliation logic)
                var result = template.UpdateInstallments(installments);
                if (result.IsError) return result.TopError;

                await unitOfWork.SaveChangesAsync(ct);
                return Result.Success;
            }
        }
    
}
