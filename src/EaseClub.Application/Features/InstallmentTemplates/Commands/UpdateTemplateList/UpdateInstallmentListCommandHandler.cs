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

                var result = template.UpdateInstallments(request.newPercentages);

                if (result.IsError) return result.TopError;

                await unitOfWork.SaveChangesAsync(ct);
                return Result.Success;
            }
        }
    
}
