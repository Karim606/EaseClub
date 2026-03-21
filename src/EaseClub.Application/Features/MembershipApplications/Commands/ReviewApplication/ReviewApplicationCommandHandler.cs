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
    public class ReviewApplicationHandler(
        IMembershipApplicationRepository appRepo,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<ReviewApplicationCommand, Result<Success>>
    {
        public async Task<Result<Success>> Handle(ReviewApplicationCommand request, CancellationToken ct)
        {
            var app = await appRepo.GetByIdAsync(request.ApplicationId, ct);
            if (app == null)
                return Error.NotFound("Application not found");

            var res = Guid.TryParse(currentUserService.GetId(),out var reviewerId);
            if(res == false)
            {
                return Error.Unauthorized();
            }
            // Create review entity
            var reviewResult = ApplicationReview.Create(
                Guid.NewGuid(),
                request.ApplicationId,
                reviewerId,
                request.Decision,
                request.RejectionReason
            );

            if (reviewResult.IsError)
            {
                return reviewResult.TopError;
            }

            var review = reviewResult.Value;

            // Apply domain logic
            var result = app.AddReview(review);
            if (result.IsError)
                return result.TopError;

            // Persist changes
            await unitOfWork.SaveChangesAsync(ct);

            return Result.Success;
        }
    }
}
