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

namespace EaseClub.Application.Features.ApplicationTemplates.Commands.Field.RemoveField
{
    public class RemoveFieldCommandHandler(
    IApplicationTemplateRepository tempRepo,
    IApplicationFieldRepository fieldRepo,
    IUnitOfWork unitOfWork) : IRequestHandler<RemoveFieldCommand, Result<Success>>
    {
        public async Task<Result<Success>> Handle(RemoveFieldCommand request, CancellationToken ct)
        {
            var temp = await tempRepo.GetFullTemplateAsync(request.TemplateId);
            if (temp == null) return Error.NotFound("Template.NotFound");

            var result = temp.RemoveField(request.SectionId, request.FieldId);
            if (result.IsError) return result.TopError;

            var field = await fieldRepo.GetByIdAsync(request.FieldId);
            await fieldRepo.DeleteAsync(field);

            await unitOfWork.SaveChangesAsync(ct);
            return Result.Success;
        }
    }
}
