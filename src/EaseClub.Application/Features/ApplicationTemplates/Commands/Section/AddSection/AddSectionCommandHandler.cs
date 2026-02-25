using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.ApplicationTemplates.Repositories;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.RepeatRule;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.ApplicationTemplates.Commands.Section.AddSection
{
    public class AddSectionCommandHandler(ILogger<AddSectionCommandHandler>logger,
        IApplicationStepRepository stepRepository,
        IApplicationSectionRepository sectionRepository,
        IUnitOfWork unitOfWork) 
    : IRequestHandler<AddSectionCommand, Result<Guid>>
    {
  

        public async Task<Result<Guid>> Handle(AddSectionCommand request, CancellationToken ct)
        {
            // 1. Fetch the Step (The Behavior already verified it belongs to the Club)
            var step = await stepRepository.GetByIdAsync(request.StepId, ct);

            if (step == null)
                return Error.NotFound("Step.NotFound", "The specified step does not exist.");

            RepeatRule? repeatRule = null;
            if (request.RepeatRuleJson != null)
            {
              var resOfRepeatRule = RepeatRule.Create(request.RepeatRuleJson.DependsOnFieldKey, request.RepeatRuleJson.Mode);

                if (resOfRepeatRule.IsError) {
                    logger.LogError("error happend while Adding section into step with id:{StepId}, Reason:{Error}", request.StepId, resOfRepeatRule.TopError);
                    return resOfRepeatRule.TopError; 
                }

                repeatRule = resOfRepeatRule.Value;
            }

            // 2. Domain Logic: Create the Section via the Step Aggregate
            var sectionResult = step.AddNewSection(
                request.Title,
                request.Order-1,
                repeatRule
            );

            if (sectionResult.IsError) {
                logger.LogError("Failed to add new section into step with id:{StepId}, reason:{Error}", request.StepId,
                    sectionResult.TopError.ToLogObject());
                return sectionResult.TopError; 
            }

            await sectionRepository.AddAsync(sectionResult.Value, ct);
            // 3. Persist
            await unitOfWork.SaveChangesAsync(ct);

            return sectionResult.Value.Id;
        }
    }
}
