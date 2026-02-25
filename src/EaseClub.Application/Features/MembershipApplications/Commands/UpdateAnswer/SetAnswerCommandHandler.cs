using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.ApplicationTemplates.Repositories;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipApplications.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipApplications.Commands.UpdateAnswer
{
    public class SetAnswerCommandHandler(
    IMembershipApplicationRepository membershipAppRepository,
    IApplicationFieldRepository fieldRepository,
    ILogger<SetAnswerCommandHandler>logger,
    IUnitOfWork unitOfWork) : IRequestHandler<SetAnswerCommand, Result<Success>>
    {
        public async Task<Result<Success>> Handle(SetAnswerCommand request, CancellationToken ct)
        {
            // 1. Load the Aggregate Root with its collection of answers
            var application = await membershipAppRepository.GetByIdWithAnswersAsync(request.ApplicationId, ct);

            if (application == null)
            {
                logger.LogWarning("Application not found.");
                return Error.NotFound("Application.NotFound", "Application not found.");
            }

            var fieldDef = await fieldRepository.GetByIdAsync(request.FieldDefinitionId, ct);
            if (fieldDef == null)
            {
                logger.LogWarning("Field definition not found.");
                return Error.NotFound("FieldDefinition.NotFound", "field definition not found.");
            }
            // 2. Delegate the logic to the Domain Model
            // This method ensures status is 'Draft' and handles the internal factory
            var result = application.SetAnswer(
                fieldDef.Key,
                request.FieldDefinitionId,
                request.Value,
                request.InstanceIndex);

            if (result.IsError)
            {
                logger.LogError("Failed to update answer in application with id:{ApplicationId}, reason:{Error}",
                    request.ApplicationId, result.TopError);
                return result.TopError;
            }


            await membershipAppRepository.UpdateAnswerAsync(application);
            await unitOfWork.SaveChangesAsync(ct);

            return Result.Success;
        }
    }
}
