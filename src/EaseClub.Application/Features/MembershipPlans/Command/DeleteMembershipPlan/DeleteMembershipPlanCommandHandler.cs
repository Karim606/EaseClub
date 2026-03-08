using EaseClub.Application.Common;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipPlans;
using EaseClub.Domain.MembershipPlans.Repositories;
using EaseClub.Domain.Memberships;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipPlans.Command.DeleteMembershipPlan
{
    public class DeleteMembershipPlanHandler(
     IMembershipPlanRepository repository,
     IUnitOfWork unitOfWork) : IRequestHandler<DeleteMembershipPlanCommand, Result<Success>>
    {
        public async Task<Result<Success>> Handle(DeleteMembershipPlanCommand request, CancellationToken ct)
        {
            // 1. Fetch the plan
            var plan = await repository.GetByIdAsync(request.Id, ct);


            // 3. Perform deletion
            await  repository.DeleteAsync(plan);
            await unitOfWork.SaveChangesAsync(ct);

            return Result.Success;
        }
    }
}
