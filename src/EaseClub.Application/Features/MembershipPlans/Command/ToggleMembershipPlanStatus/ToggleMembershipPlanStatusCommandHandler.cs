using Microsoft.Extensions.Logging;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.MembershipPlans;
using EaseClub.Domain.MembershipPlans.Repositories;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipPlans.Command.ToggleMembershipPlanStatus
{
    public class ToggleMembershipPlanStatusCommandHandler(
        IMembershipPlanRepository repository, 
        IUnitOfWork unitOfWork,
        ILogger<ToggleMembershipPlanStatusCommandHandler> logger) 
        : IRequestHandler<ToggleMembershipPlanStatusCommand, Result<Success>>
    {
        public async Task<Result<Success>> Handle(ToggleMembershipPlanStatusCommand request, CancellationToken ct)
        {
            var plan = await repository.GetByIdAsync(request.Id, ct);
            if (plan is null)
            {
                logger.LogError("NotFound error in ToggleMembershipPlanStatusCommandHandler: Plan {PlanId} not found.", request.Id);
                return Error.NotFound("Membership plan not found.");
            }

            if (plan.IsActive)
                plan.Deactivate();
            else
                plan.Activate();

            await unitOfWork.SaveChangesAsync(ct);
            return Result.Success;
        }
    }
}
