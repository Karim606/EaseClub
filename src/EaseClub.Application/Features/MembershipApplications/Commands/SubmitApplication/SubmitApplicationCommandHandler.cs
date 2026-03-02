using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
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

            // 3. Persist the "Locked" state to the JSON columns
            await unitOfWork.SaveChangesAsync(ct);


            return Result.Success;
        }
    }
}
