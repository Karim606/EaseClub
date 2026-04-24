using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipPlans;
using EaseClub.Domain.MembershipPlans.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EaseClub.Application.Features.MembershipPlans.Command.SyncInstallmentTemplates
{
    public class SyncMembershipPlanInstallmentTemplatesCommandHandler(
        IMembershipPlanRepository membershipPlanRepository,
        IInstallmentsTemplatesRepository installmentsTemplatesRepository,
        IUnitOfWork unitOfWork,
        ILogger<SyncMembershipPlanInstallmentTemplatesCommandHandler> logger)
        : IRequestHandler<SyncMembershipPlanInstallmentTemplatesCommand, Result<Success>>
    {
        public async Task<Result<Success>> Handle(SyncMembershipPlanInstallmentTemplatesCommand request, CancellationToken cancellationToken)
        {
            var plan = await membershipPlanRepository.GetPlanWithDetailsAsync(request.PlanId, cancellationToken);
            if (plan is null) { logger.LogError("NotFound error in SyncMembershipPlanInstallmentTemplatesCommandHandler: {Error}", Error.NotFound(description: "Membership plan not found.").ToLogObject()); return Error.NotFound(description: "Membership plan not found."); }
            var requestedTemplateIds = request.InstallmentTemplateIds ?? [];
            var templates = new List<InstallmentTemplate>();
            foreach (var templateId in requestedTemplateIds)
            {
                var template = await installmentsTemplatesRepository.GetByIdAsync(templateId, cancellationToken);
                if (template is null)
                {
                    logger.LogWarning("Installment template with id:{InstallmentTemplateId} not found.", templateId);
                    return Error.NotFound(description: $"Installment template with id:{templateId} not found.");
                }

                templates.Add(template);
            }
            var syncResult = plan.SyncInstallmentTemplates(templates);
            if (syncResult.IsError) { logger.LogError("Error in SyncMembershipPlanInstallmentTemplatesCommandHandler: {Error}", syncResult.TopError.ToLogObject()); return syncResult.TopError; }
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success;
        }
    }
}
