using Microsoft.Extensions.Logging;
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
        IUnitOfWork unitOfWork,
        ILogger<UpdateInstallmentListCommandHandler> logger) : IRequestHandler<UpdateInstallmentListCommand, Result<Success>>
        {
           
            public async Task<Result<Success>> Handle(UpdateInstallmentListCommand request, CancellationToken ct)
            {
                var template = await repository.GetByIdAsync(request.TemplateId, ct);
                if (template is null) { logger.LogError("NotFound error in UpdateInstallmentListCommandHandler: {Error}", Error.NotFound("Template not found.").ToLogObject()); return Error.NotFound("Template not found."); }
            var result = template.UpdateInstallments(request.newPercentages);

                if (result.IsError) { logger.LogError("Error in UpdateInstallmentListCommandHandler: {Error}", result.TopError.ToLogObject()); return result.TopError; }
            await unitOfWork.SaveChangesAsync(ct);
                return Result.Success;
            }
        }
    
}
