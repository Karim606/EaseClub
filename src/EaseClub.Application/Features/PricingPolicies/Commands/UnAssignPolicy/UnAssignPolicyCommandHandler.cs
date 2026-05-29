using Microsoft.Extensions.Logging;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.ApplicationTemplates.Repositories;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Events;
using EaseClub.Domain.PricingPolices;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.PricingPolicies.Commands.UnAssignPolicy
{
    public class UnassignPolicyCommandHandler(
        IApplicationTemplateRepository templateRepo,
        IEventRepository eventRepo,
        IPricingPolicyRepository policyRepo,
        IUnitOfWork unitOfWork,
        ILogger<UnassignPolicyCommandHandler> logger)
     : IRequestHandler<UnAssignPolicyCommand, Result<Success>>
    {
        public async Task<Result<Success>> Handle(
            UnAssignPolicyCommand request,
            CancellationToken ct)
        {
            switch (request.TargetType)
            {
                case PricingPolicyTargetType.ApplicationTemplate:

                    var template = await templateRepo.GetFullTemplateAsync(request.TargetId, ct);
                    if (template == null)
                    {
                        logger.LogError("NotFound error in UnassignPolicyCommandHandler: {Error}", Error.NotFound("Template.NotFound").ToLogObject());
                        return Error.NotFound("Template.NotFound");
                    }

                    var templateAssignmentResult = template.UnAssignPolicy(request.PolicyId);
                    if (templateAssignmentResult.IsError) { logger.LogError("Error in UnassignPolicyCommandHandler: {Error}", templateAssignmentResult.TopError.ToLogObject()); return templateAssignmentResult.TopError; }
                    policyRepo.DeleteAssignmentAsync(templateAssignmentResult.Value, ct);
                    break;

                case PricingPolicyTargetType.Event:

                    var @event = await eventRepo.GetByIdAsync(request.TargetId, ct);
                    if (@event == null)
                    {
                        logger.LogError("NotFound error in UnassignPolicyCommandHandler: {Error}", Error.NotFound("Event.NotFound").ToLogObject());
                        return Error.NotFound("Event.NotFound");
                    }

                    var unassignResult = @event.UnassignPricingPolicy(request.PolicyId);
                    if (unassignResult.IsError) { logger.LogError("Error in UnassignPolicyCommandHandler: {Error}", unassignResult.TopError.ToLogObject()); return unassignResult.TopError; }

                    var eventAssignment = await policyRepo.GetAssignmentAsync(request.TargetId, request.PolicyId, ct);
                    if (eventAssignment != null)
                        policyRepo.DeleteAssignmentAsync(eventAssignment, ct);
                    break;

                default:
                    return Error.Validation("InvalidTargetType");
            }

            await unitOfWork.SaveChangesAsync(ct);
            return Result.Success;
        }
    }
}
