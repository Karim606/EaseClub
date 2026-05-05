using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Member;
using EaseClub.Domain.Memberships;
using MediatR;

namespace EaseClub.Application.Features.Enrollments.Commands.Cancel
{
    public class CancelEnrollmentCommandHandler(
         ICurrentUserService currentUserService,
         IEnrollmentRepository enrollmentRepository,
         IMemberUserRepository memberUserRepository,
         IUnitOfWork unitOfWork) : IRequestHandler<CancelEnrollmentCommand, Result<Success>>
    {
        public async Task<Result<Success>> Handle(CancelEnrollmentCommand request, CancellationToken ct)
        {
            var member = await memberUserRepository.GetByIdAsync(request.MemberId, ct);
            if (member == null) return Error.NotFound(description: "Member not found.");

            var enrollment = await enrollmentRepository.GetByIdAsync(request.EnrollmentId, ct);
            if (enrollment == null) return Error.NotFound();

            var res = enrollment.MarkCancelled();
            if (res.IsError) return res.TopError;

            await unitOfWork.SaveChangesAsync(ct);

            return Result.Success;
        }
    }
}
