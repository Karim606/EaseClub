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

namespace EaseClub.Application.Features.ApplicationTemplates.Commands.Section.RemoveSection
{
    public class RemoveSectionCommandHandler(
     IApplicationStepRepository stepRepository,
     IApplicationSectionRepository secRepo,
     IUnitOfWork unitOfWork) : IRequestHandler<RemoveSectionCommand, Result<Success>>
    {
        public async Task<Result<Success>> Handle(RemoveSectionCommand request, CancellationToken ct)
        {
            // Ensure sections are included to allow the Domain reordering logic to work
            var step = await stepRepository.GetStepWithSections(request.StepId, ct);
            if (step == null) return Error.NotFound("Step.NotFound");

            var result = step.RemoveSection(request.SectionId);
            if (result.IsError) return result.TopError;

            var sec = await secRepo.GetByIdAsync(request.SectionId);
            await secRepo.DeleteAsync(sec);

            await unitOfWork.SaveChangesAsync(ct);
            return Result.Success;
        }
    }
}
