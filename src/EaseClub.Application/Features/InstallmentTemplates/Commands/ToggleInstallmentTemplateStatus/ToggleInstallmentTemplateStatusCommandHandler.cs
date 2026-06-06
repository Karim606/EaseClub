using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipPlans.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EaseClub.Application.Features.InstallmentTemplates.Commands.ToggleInstallmentTemplateStatus
{
    public class ToggleInstallmentTemplateStatusCommandHandler(
        IInstallmentsTemplatesRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<ToggleInstallmentTemplateStatusCommandHandler> logger)
        : IRequestHandler<ToggleInstallmentTemplateStatusCommand, Result<Success>>
    {
        public async Task<Result<Success>> Handle(ToggleInstallmentTemplateStatusCommand request, CancellationToken ct)
        {
            var template = await repository.GetByIdAsync(request.Id, ct);
            if (template is null)
            {
                logger.LogError("NotFound error in ToggleInstallmentTemplateStatusCommandHandler: InstallmentTemplate {Id} not found.", request.Id);
                return Error.NotFound("Installment template not found.");
            }

            if (template.IsActive)
                template.Deactivate();
            else
                template.Activate();

            await unitOfWork.SaveChangesAsync(ct);
            return Result.Success;
        }
    }
}
