using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Features.ApplicationTemplates.Services;
using EaseClub.Domain.ApplicationTemplates;
using EaseClub.Domain.ApplicationTemplates.Repositories;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.ApplicationTemplates.Commands.Template.UpsertTemplate
{
    public class UpsertTemplateCommandHandler(
        IApplicationTemplateRepository tempRepo,
        IUnitOfWork unitOfWork) : IRequestHandler<UpsertTemplateCommand, Result<Success>>
    {

        public async Task<Result<Success>> Handle(UpsertTemplateCommand request, CancellationToken cancellationToken)
        {
            ApplicationTemplateDefinition template;

            // 1️⃣ Load existing template or create new
            if (request.TemplateId.HasValue)
            {
                template = await tempRepo.GetFullTemplateAsync(request.TemplateId.Value);

                if (template == null) return Error.NotFound("Template.NotFound");
                if (template.ClubId != request.ClubId) return Error.Forbidden("Template.Forbidden");

                var updateResult = template.Update(request.Name);
                if (updateResult.IsError) return updateResult.TopError;
            }
            else
            {
                var createResult = ApplicationTemplateDefinition.Create(
                    Guid.NewGuid(),
                    request.ClubId,
                    request.Name
                );
                if (createResult.IsError) return createResult.TopError;

                template = createResult.Value;
                await tempRepo.AddAsync(template);
            }

            // 2️⃣ Delegate all upsert logic to TemplateUpdater
            var updater = new TemplateUpdater(template);
            var upsertResult = updater.ApplySteps(request.Steps);
            if (upsertResult.IsError) return upsertResult.TopError;

            var validation = template.ValidateConsistency();
            if (validation.IsError) return validation.TopError;
            // 3️⃣ Persist changes
            await unitOfWork.SaveChangesAsync();

            return Result.Success;

        }
    }
}
