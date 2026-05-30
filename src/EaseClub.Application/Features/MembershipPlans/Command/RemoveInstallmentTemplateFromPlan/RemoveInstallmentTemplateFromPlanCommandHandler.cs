using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipPlans.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipPlans.Command.RemoveInstallmentTemplateFromPlan
{
    public class RemoveInstallmentTemplateFromPlanCommandHandler(IMembershipPlanRepository membershipPlanRepository,
        IInstallmentsTemplatesRepository installmentsTemplatesRepository,
        ILogger<RemoveInstallmentTemplateFromPlanCommandHandler>logger,IUnitOfWork unitOfWork)
        : IRequestHandler<RemoveInstallmentTemplateFromPlanCommand, Result<Success>>
    {
        public async Task<Result<Success>> Handle(RemoveInstallmentTemplateFromPlanCommand request, CancellationToken cancellationToken)
        {
            var installmentTemplate = await installmentsTemplatesRepository.GetByIdAsync(request.TemplateId);

            if (installmentTemplate == null) {
                logger.LogWarning("installment template not found for club with id={ClubId}", request.ClubId);
                return Error.NotFound(description: "installment template not found ");
            }
            var plan = await membershipPlanRepository.GetPlanWithDetailsAsync(request.PlanId);
            if (plan == null) {
                logger.LogWarning("plan not found for club with id={ClubId}", request.ClubId);
                return Error.NotFound(description: "plan is not found");
            }

            plan.RemoveInstallmentTemplate(request.TemplateId);
            await unitOfWork.SaveChangesAsync();
            return Result.Success;
        }
    }
}
