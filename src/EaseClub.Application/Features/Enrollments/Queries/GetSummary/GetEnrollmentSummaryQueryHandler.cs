using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Clubs;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipPlans.Repositories;
using EaseClub.Domain.Memberships;
using MediatR;

namespace EaseClub.Application.Features.Enrollments.Queries.GetSummary
{
    public class GetEnrollmentSummaryQueryHandler(
        ICurrentUserService currentUserService,
        IEnrollmentRepository enrollmentRepository,
        IClubRepository clubRepository,
        IMembershipPlanRepository planRepository)
        : IRequestHandler<GetEnrollmentSummaryQuery, Result<EnrollmentSummaryDto>>
    {
        public async Task<Result<EnrollmentSummaryDto>> Handle(GetEnrollmentSummaryQuery request, CancellationToken ct)
        {
            var enrollment = await enrollmentRepository.GetByIdWithDetailsAsync(request.EnrollmentId, ct);
            if (enrollment == null) return Error.NotFound(description: "Enrollment not found.");

            var installments = enrollment.GetInstallments();

            var summaries = installments.Select(inst => new InstallmentSummaryDto(
                inst.Amount,
                inst.AfterDueInDays,
                inst.Order)).ToList();

            return new EnrollmentSummaryDto(
                enrollment.Id,
                enrollment.Club?.Name ?? "Unknown Club",
                enrollment.MembershipPlan?.Name ?? "Unknown Plan",
                enrollment.TotalPrice,
                enrollment.Amount,
                summaries,
                enrollment.ExpiresAt,
                enrollment.Source.ToString());
        }
    }
}
