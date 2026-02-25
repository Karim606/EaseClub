using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.ApplicationTemplates.Repositories;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.ApplicationTemplates.Commands.Template.UpdateTemplate
{
    public class UpdateTemplateCommandHandler(IApplicationTemplateRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateTemplateCommandHandler>logger)
     : IRequestHandler<UpdateTemplateCommand, Result<Success>>
    {
        public async Task<Result<Success>> Handle(UpdateTemplateCommand request, CancellationToken ct)
        {
            var template = await repository.GetByIdAsync(request.TemplateId, ct);
            if (template == null) return Error.NotFound("Template.NotFound", "Template not found.");


            var res = template.Update(request.Name);

            if (res.IsError)
            {
                logger.LogError("Failed to update template with id:{TemplateId}, reason:{Error}", request.TemplateId,res.TopError);
                return res.TopError;
            }

            await unitOfWork.SaveChangesAsync(ct);
            return Result.Success;
        }
    }
}
