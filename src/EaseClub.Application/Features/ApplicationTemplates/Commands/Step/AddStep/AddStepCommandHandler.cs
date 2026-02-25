using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.ApplicationTemplates.Repositories;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.ApplicationTemplates.Commands.Step.AddStep
{
    public class AddStepCommandHandler(ILogger<AddStepCommandHandler>logger,
        IApplicationTemplateRepository templateRepository,
        IApplicationStepRepository stepRepository,
        IUnitOfWork _unitOfWork
        )
        : IRequestHandler<AddStepCommand, Result<Guid>>
    {

        public async Task<Result<Guid>> Handle(AddStepCommand request, CancellationToken ct)
        {
            var template = await templateRepository.GetTemplateWithStepsAsync(request.TemplateId, ct);

            if (template == null)
                return Error.NotFound("Template.NotFound", "The template does not exist.");

             

            var stepResult = template.AddNewStep(
                request.Category,
                request.Title,
                request.Order-1
            );

            if (stepResult.IsError)
                return stepResult.TopError;

            await stepRepository.AddAsync(stepResult.Value, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return stepResult.Value.Id;
        }
    }
}
