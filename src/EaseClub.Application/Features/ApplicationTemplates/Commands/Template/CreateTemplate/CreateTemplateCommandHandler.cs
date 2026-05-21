using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.ApplicationTemplates;
using EaseClub.Domain.ApplicationTemplates.Repositories;
using EaseClub.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.ApplicationTemplates.Commands.Template.CreateTemplate
{
    public class CreateTemplateCommandHandler(IApplicationTemplateRepository repository,
        IUnitOfWork unitOfWork,ILogger<CreateTemplateCommandHandler>logger)
    : IRequestHandler<CreateTemplateCommand, Result<Guid>>
    {

        public async Task<Result<Guid>> Handle(CreateTemplateCommand request, CancellationToken ct)
        {
            // The Domain Factory handles the validation of the ClubId (e.g., not empty)
            var templateResult = ApplicationTemplateDefinition.Create(
                Guid.NewGuid(),
                request.ClubId,
                 request.Name
            );

            if (templateResult.IsError) {
                logger.LogError("Creation of application-template failed Error: {Error}", templateResult.TopError);
                return templateResult.TopError; }
            await repository.AddAsync(templateResult.Value, ct);
            await unitOfWork.SaveChangesAsync(ct);

            return templateResult.Value.Id;
        }
    }
}
