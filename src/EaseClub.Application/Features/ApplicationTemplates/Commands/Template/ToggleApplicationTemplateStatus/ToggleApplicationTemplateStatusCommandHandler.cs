using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.ApplicationTemplates.Repositories;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EaseClub.Application.Features.ApplicationTemplates.Commands.Template.ToggleApplicationTemplateStatus
{
    public class ToggleApplicationTemplateStatusCommandHandler(
        IApplicationTemplateRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<ToggleApplicationTemplateStatusCommandHandler> logger)
        : IRequestHandler<ToggleApplicationTemplateStatusCommand, Result<Success>>
    {
        public async Task<Result<Success>> Handle(ToggleApplicationTemplateStatusCommand request, CancellationToken ct)
        {
            var template = await repository.GetByIdAsync(request.Id, ct);
            if (template is null)
            {
                logger.LogError("NotFound error in ToggleApplicationTemplateStatusCommandHandler: ApplicationTemplate {Id} not found.", request.Id);
                return Error.NotFound("Application template not found.");
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
