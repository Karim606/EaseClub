using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Features.ApplicationTemplates.Queries.GetStepByOrder;
using EaseClub.Domain.ApplicationTemplates.Repositories;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.ApplicationTemplates.Commands.Section.UpdateSection
{
    public class UpdateSectionCommandHandler(IApplicationSectionRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateSectionCommandHandler>logger)
        : IRequestHandler<UpdateSectionCommand, Result<Success>>
    {
        public async Task<Result<Success>> Handle(UpdateSectionCommand request, CancellationToken cancellationToken)
        {
            var section = await repository.GetByIdAsync(request.SectionId, cancellationToken);
            if (section == null) return Error.NotFound("Section.NotFound");

            // Domain method to update properties
            var res =  section.Update(request.Title, request.RepeatRuleJson);

            if (res.IsError)
            {
                logger.LogError("Failed to update section with id:{SectionId}, reason:{Error}",request.SectionId,res.TopError);
                return res.TopError;
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success;
        }
    }
}
