using Microsoft.Extensions.Logging;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Features.MembershipPlans.Command.AddTemplateToPlan;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipPlans;
using EaseClub.Domain.MembershipPlans.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipPlans.Command.AddInstallmentTemplateToPlan
{
    public class AddInstallmentTemplateToPlanHandler(IMembershipPlanRepository planRepository,
        IInstallmentsTemplatesRepository installmentsTemplatesRepository,
        IUnitOfWork unitOfWork,
        ILogger<AddInstallmentTemplateToPlanHandler> logger)
    : IRequestHandler<AddInstallmentTemplateToPlanCommand, Result<Success>>
    {

        public async Task<Result<Success>> Handle(AddInstallmentTemplateToPlanCommand request, CancellationToken cancellationToken)
        {
            var plan = await planRepository.GetByIdAsync(request.PlanId);
            if (plan is null) return MembershipPlanErrors.NotFound;

            // Domain Logic: Plan ensures it doesn't add the same template twice
            var template = await installmentsTemplatesRepository.GetByIdAsync(request.TemplateId);
            if (template == null) return MembershipPlanErrors.InstallmentTemplateDoesntExist;

            var result = plan.AddInstallmentTemplate(template);
            if (result.IsError) { logger.LogError("Error in AddInstallmentTemplateToPlanHandler: {Error}", result.TopError.ToLogObject()); return result.TopError; }
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success;
        }
    }
}
