using Microsoft.Extensions.Logging;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.ConditionExpression;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.PricingPolices;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.PricingPolicies.Commands.UpdatePolicy
{
    public class UpdatePricingPolicyCommandHandler(IPricingPolicyRepository policyRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdatePricingPolicyCommandHandler> logger) : IRequestHandler<UpdatePricingPolicyCommand, Result<Success>>
    {

        public async Task<Result<Success>> Handle(UpdatePricingPolicyCommand request, CancellationToken ct)
        {
            var policy = await policyRepository.GetByIdAsync(request.Id, ct);
            if (policy == null) { logger.LogError("NotFound error in UpdatePricingPolicyCommandHandler: {Error}", Error.NotFound(description: "Policy not found").ToLogObject()); return Error.NotFound(description: "Policy not found"); }
            var conditions = new List<ConditionExpression>();
            foreach (var c in request.Conditions)
            {
                var condResult = ConditionExpression.Create(c.FieldKey, c.Operator, c.ExpectedValue);
                if (condResult.IsError) { logger.LogError("Error in UpdatePricingPolicyCommandHandler: {Error}", condResult.TopError.ToLogObject()); return condResult.TopError; }
            conditions.Add(condResult.Value);
            }
            var updateResult = policy.Update(
                request.Name,
                request.Priority,
                request.IsIncrease,
                request.FixedAmount,
                request.PercentageValue,
                request.MultiplierKey,
                conditions);

            if (updateResult.IsError) { logger.LogError("Error in UpdatePricingPolicyCommandHandler: {Error}", updateResult.TopError.ToLogObject()); return updateResult.TopError; }
            await unitOfWork.SaveChangesAsync(ct);
            return Result.Success;
        }
    }
}
