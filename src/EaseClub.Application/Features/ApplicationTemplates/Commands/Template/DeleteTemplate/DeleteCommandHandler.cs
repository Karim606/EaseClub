using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.ApplicationTemplates.Repositories;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.ApplicationTemplates.Commands.Template.DeleteTemplate
{
    public class DeleteTemplateCommandHandler(IApplicationTemplateRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteTemplateCommand, Result<Success>>
    {
        public async Task<Result<Success>> Handle(DeleteTemplateCommand request, CancellationToken ct)
        {
            var template = await repository.GetFullTemplateAsync(request.TemplateId, ct);
            if (template == null) return Error.NotFound("Template.NotFound", "Template not found.");

            await repository.DeleteAsync(template);
            await unitOfWork.SaveChangesAsync(ct);

            return Result.Success;
        }
    }
}
