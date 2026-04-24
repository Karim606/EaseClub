using Microsoft.Extensions.Logging;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Features.PricingPolicies.Commands.AssignPolicy;
using EaseClub.Domain.ApplicationTemplates.Repositories;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.PricingPolices;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.PricingPolicies.Commands.UnAssignPolicy
{
    public class UnassignPolicyCommandHandler(IApplicationTemplateRepository templateRepo,
     IUnitOfWork unitOfWork,
        ILogger<UnassignPolicyCommandHandler> logger)
     : IRequestHandler<UnAssignPolicyCommand, Result<Success>>
    {
        public async Task<Result<Success>> Handle(
            UnAssignPolicyCommand request,
            CancellationToken ct)
        {
            Result<Success> res;

            switch (request.TargetType)
            {
                case PricingPolicyTargetType.ApplicationTemplate:

                    var template = await templateRepo.GetFullTemplateAsync(request.TargetId);
                    if (template == null) { logger.LogError("NotFound error in UnassignPolicyCommandHandler: {Error}", Error.NotFound("Template.NotFound").ToLogObject()); return Error.NotFound("Template.NotFound"); }

                    res = template.UnAssignPolicy(request.PolicyId);
                    break;

                default:
                    return Error.Validation("InvalidTargetType");
            }
            if (res.IsError) { logger.LogError("Error in UnassignPolicyCommandHandler: {Error}", res.TopError.ToLogObject()); return res.TopError; }
            await unitOfWork.SaveChangesAsync(ct);

            return Result.Success;
        }
    }
}
