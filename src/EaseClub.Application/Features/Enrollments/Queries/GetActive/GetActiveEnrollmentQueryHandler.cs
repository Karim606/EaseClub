using EaseClub.Domain.Clubs;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipPlans.Repositories;
using EaseClub.Domain.Memberships;
using MediatR;

namespace EaseClub.Application.Features.Enrollments.Queries.GetActive
{
    public class GetActiveEnrollmentQueryHandler(
       IEnrollmentRepository enrollmentRepository,
       IClubRepository clubRepository,
       IMembershipPlanRepository planRepository)
       : IRequestHandler<GetActiveEnrollmentQuery, Result<ActiveEnrollmentDto?>>
    {
        public async Task<Result<ActiveEnrollmentDto?>> Handle(GetActiveEnrollmentQuery request, CancellationToken ct)
        {
            var active = await enrollmentRepository.GetLatestActiveByUserIdAsync(request.MemberId, ct);

            if (active == null) return (ActiveEnrollmentDto?)null;

            return new ActiveEnrollmentDto(
                active.Id,
                active.Club?.Name ?? "Unknown Club",
                active.MembershipPlan?.Name ?? "Unknown Plan",
                active.TotalPrice,
                active.Amount,
                active.ExpiresAt,
                active.Source.ToString());
        }
    }
}
