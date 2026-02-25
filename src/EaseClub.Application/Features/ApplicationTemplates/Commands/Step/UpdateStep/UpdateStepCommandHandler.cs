using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.ApplicationTemplates.Repositories;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.ApplicationTemplates.Commands.Step.UpdateStep
{
    public class UpdateStepCommandHandler(IApplicationStepRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateStepCommandHandler> logger) 
        : IRequestHandler<UpdateStepCommand, Result<Success>>
    {
        public async Task<Result<Success>> Handle(UpdateStepCommand request, CancellationToken cancellationToken)
        {
            var step = await repository.GetByIdAsync(request.StepId, cancellationToken);
            if (step == null) return Error.NotFound("Step.NotFound", "Step not found.");

            var res = step.Update(request.Category, request.Title);
            
            if (res.IsError)
            {
                logger.LogError("Failed to update step with id:{StepId}, reason:{Error}",request.StepId,res.TopError);
                return res.TopError;
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success;
        }
    }
}
