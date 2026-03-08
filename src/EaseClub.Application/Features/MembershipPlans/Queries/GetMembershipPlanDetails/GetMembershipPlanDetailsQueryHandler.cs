using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipPlans;
using EaseClub.Domain.MembershipPlans.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipPlans.Queries.GetMembershipPlanDetails
{
    public class GetMembershipPlanDetailsHandler(IMembershipPlanRepository membershipPlanRepository,
        IInstallmentsTemplatesRepository installmentsTemplatesRepository,
        ILogger<GetMembershipPlanDetailsHandler> logger)
        : IRequestHandler<GetMembershipPlanDetailsQuery, Result<MembershipPlanDetailsDto>>
    {

        public async Task<Result<MembershipPlanDetailsDto>> Handle(GetMembershipPlanDetailsQuery request, CancellationToken cancellationToken)
        {
            var plan = await membershipPlanRepository.GetByIdAsync(request.PlanId, cancellationToken);

            if (plan == null)
            {
                logger.LogWarning("plan with id={PlanId} is not found", request.PlanId);
                return Error.NotFound(description: "plan is not found");
            }

               var temp = await installmentsTemplatesRepository.GetByPlanIdAsync(plan.Id, cancellationToken);

           var dto = new MembershipPlanDetailsDto(plan.Id,plan.Name,plan.Description,plan.DurationInDays,plan.TotalPrice,plan.IsActive,
                temp.Select( x => new InstallmentsTemplateDto(x.Id,x.Name)).ToList());
           
            return dto;
        }
    }
}
