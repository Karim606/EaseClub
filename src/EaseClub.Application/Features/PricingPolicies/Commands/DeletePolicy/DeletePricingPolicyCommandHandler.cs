using Microsoft.Extensions.Logging;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.PricingPolices;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.PricingPolicies.Commands.DeletePolicy
{
    public class DeletePricingPolicyCommandHandler(IPricingPolicyRepository policyRepo,
        IUnitOfWork unitOfWork,
        ILogger<DeletePricingPolicyCommandHandler> logger) : IRequestHandler<DeletePricingPolicyCommand, Result<Success>>
    {
        public async Task<Result<Success>> Handle(DeletePricingPolicyCommand request, CancellationToken cancellationToken)
        {
           var policy = await policyRepo.GetByIdAsync(request.Id);
            if (policy == null) { logger.LogError("NotFound error in DeletePricingPolicyCommandHandler: {Error}", Error.NotFound(description: "Policy not found").ToLogObject()); return Error.NotFound(description: "Policy not found"); }
            await policyRepo.DeleteAsync(policy,cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success;
        }
    }
}
