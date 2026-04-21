using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipPlans;
using EaseClub.Domain.MembershipPlans.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipPlans.Command.UpdatePlan
{
    public class UpdateMembershipPlanHandler(
    IMembershipPlanRepository repository,
    IUnitOfWork unitOfWork,
    ILogger<UpdateMembershipPlanHandler>logger) : IRequestHandler<UpdateMembershipPlanCommand, Result<Success>>
    {
        public async Task<Result<Success>> Handle(UpdateMembershipPlanCommand request, CancellationToken ct)
        {
            // 1. Fetch the Aggregate
            var plan = await repository.GetPlanWithDetailsAsync(request.PlanId, ct);

            if (plan is null )
                return Error.NotFound("Membership plan not found.");

            // 2. Delegate to Domain Entity
            var updateResult = plan.Update(
                request.Name,
                request.Description,
                request.TotalPrice,
                request.RenewPrice
                );

            if (updateResult.IsError) { 
                logger.LogError($"Error updating membership plan: {updateResult.TopError.ToLogObject}");
                return updateResult.TopError; }

            // 3. Persist Changes
            await unitOfWork.SaveChangesAsync(ct);

            return Result.Success;
        }
    }
}
