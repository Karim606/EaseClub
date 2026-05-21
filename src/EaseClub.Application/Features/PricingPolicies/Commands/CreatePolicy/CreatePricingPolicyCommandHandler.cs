using Microsoft.Extensions.Logging;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.ConditionExpression;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.PricingPolices;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.PricingPolicies.Commands.CreatePolicy
{
    public class CreatePricingPolicyCommandHandler(IPricingPolicyRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<CreatePricingPolicyCommandHandler> logger) : IRequestHandler<CreatePricingPolicyCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(CreatePricingPolicyCommand request, CancellationToken ct)
        {
            var conditions = new List<ConditionExpression>();
            if (request.Conditions != null)
            {
                foreach (var c in request.Conditions)
                {
                    var condResult = ConditionExpression.Create(c.FieldKey, c.Operator, c.ExpectedValue);
                    if (condResult.IsError) { logger.LogError("Error in CreatePricingPolicyCommandHandler: {Error}", condResult.TopError.ToLogObject()); return condResult.TopError; }
            conditions.Add(condResult.Value);
                }
            }
            var policyResult = PricingPolicy.Create(
                Guid.NewGuid(),
                request.ClubId,
                request.Name,
                request.IsIncrease,
                request.FixedAmount,
                request.PercentageValue,
                request.MultiplierKey,
                conditions);

            if (policyResult.IsError) { logger.LogError("Error in CreatePricingPolicyCommandHandler: {Error}", policyResult.TopError.ToLogObject()); return policyResult.TopError; }
            await repository.AddAsync(policyResult.Value, ct);
            await unitOfWork.SaveChangesAsync(ct);

            return policyResult.Value.Id;
        }
    }

}
