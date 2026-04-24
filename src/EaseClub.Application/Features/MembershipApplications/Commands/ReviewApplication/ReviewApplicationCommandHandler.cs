using Microsoft.Extensions.Logging;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipApplications;
using EaseClub.Domain.MembershipApplications.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipApplications.Commands.ReviewApplication
{
    public class ReviewApplicationHandler(IMembershipApplicationRepository appRepo,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger<ReviewApplicationHandler> logger) : IRequestHandler<ReviewApplicationCommand, Result<Success>>
    {
        public async Task<Result<Success>> Handle(ReviewApplicationCommand request, CancellationToken ct)
        {
            var app = await appRepo.GetByIdAsync(request.ApplicationId, ct);
            if (app == null) { logger.LogError("NotFound error in ReviewApplicationHandler: {Error}", Error.NotFound("Application not found").ToLogObject()); return Error.NotFound("Application not found"); }
            var res = Guid.TryParse(currentUserService.GetId(),out var reviewerId);
            if (res == false) { logger.LogError("Unauthorized error in ReviewApplicationHandler: {Error}", Error.Unauthorized().ToLogObject()); return Error.Unauthorized(); }
            // Create review entity
            var reviewResult = ApplicationReview.Create(
                Guid.NewGuid(),
                request.ApplicationId,
                reviewerId,
                request.Decision,
                request.RejectionReason
            );

            if (reviewResult.IsError) { logger.LogError("Error in ReviewApplicationHandler: {Error}", reviewResult.TopError.ToLogObject()); return reviewResult.TopError; }
            var review = reviewResult.Value;

            // Apply domain logic
            var result = app.AddReview(review);
            if (result.IsError) { logger.LogError("Error in ReviewApplicationHandler: {Error}", result.TopError.ToLogObject()); return result.TopError; }// Persist changes
            await unitOfWork.SaveChangesAsync(ct);

            return Result.Success;
        }
    }
}
