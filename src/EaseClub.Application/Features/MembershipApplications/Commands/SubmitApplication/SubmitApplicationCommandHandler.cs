using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.ApplicationTemplates;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Files;
using EaseClub.Domain.MembershipApplications.Errors;
using EaseClub.Domain.MembershipApplications.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipApplications.Commands.SubmitApplication
{
    public class SubmitApplicationHandler(
    IMembershipApplicationRepository appRepo,
    IUnitOfWork unitOfWork,
    IFileRepository _fileRepository,
    IPublisher publisher) : IRequestHandler<SubmitApplicationCommand, Result<Success>>
    {
        public async Task<Result<Success>> Handle(SubmitApplicationCommand request, CancellationToken ct)
        {
            // 1. Fetch the Application with Answers
            var app = await appRepo.GetByIdAsync(request.ApplicationId, ct);
            if (app == null) return Error.NotFound(description: "Application not Found");

            // 2. Execute Domain Logic (Validation + Price Locking)
            // This runs the ValidateSectionCounts() and RefreshPrice() we built earlier
            var result = app.Submit();
            if (result.IsError) return result;

            foreach (var answer in app.Answers)
            {
                if (answer.FieldType == FieldType.File)
                {
                    var fileId = Guid.Parse(answer.Value);

                    var file = await _fileRepository.GetByIdAsync(fileId);
                    if (file == null)
                        return Error.NotFound("File missing");

                    file.MarkAsPermanent();
                }
            }

            // 3. Persist the "Locked" state to the JSON columns
            await unitOfWork.SaveChangesAsync(ct);


            return Result.Success;
        }
    }
}
