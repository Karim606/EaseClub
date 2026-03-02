using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.ApplicationTemplates.Repositories;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.ApplicationTemplates.Commands.Step.RemoveStep
{
    public class RemoveStepCommandHandler(
    IApplicationTemplateRepository templateRepository,
    IApplicationStepRepository stepRepo,
    IUnitOfWork unitOfWork) : IRequestHandler<RemoveStepCommand, Result<Success>>
    {
        public async Task<Result<Success>> Handle(RemoveStepCommand request, CancellationToken ct)
        {
            var template = await templateRepository.GetTemplateWithStepsAsync(request.TemplateId, ct);
            if (template == null) return Error.NotFound("Template.NotFound");

            var result = template.RemoveStep(request.StepId);
            if (result.IsError) return result.TopError;

            var step = await stepRepo.GetByIdAsync(request.StepId,ct);
            await stepRepo.DeleteAsync(step);

            await unitOfWork.SaveChangesAsync(ct);
            return Result.Success;
        }
    }
}
