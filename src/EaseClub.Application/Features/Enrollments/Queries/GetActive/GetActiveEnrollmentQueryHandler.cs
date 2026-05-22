using EaseClub.Domain.Clubs;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipPlans.Repositories;
using EaseClub.Domain.Memberships;
using MediatR;
using System.Collections.Generic;
using System.Linq;

namespace EaseClub.Application.Features.Enrollments.Queries.GetActive
{
    public class GetActiveEnrollmentQueryHandler(
       IEnrollmentRepository enrollmentRepository,
       IClubRepository clubRepository,
       IMembershipPlanRepository planRepository)
       : IRequestHandler<GetActiveEnrollmentQuery, Result<List<ActiveEnrollmentDto>>>
    {
        public async Task<Result<List<ActiveEnrollmentDto>>> Handle(GetActiveEnrollmentQuery request, CancellationToken ct)
        {
            var activeEnrollments = await enrollmentRepository.GetActiveByUserIdAsync(request.MemberId, ct);

            var dtos = activeEnrollments.Select(active => new ActiveEnrollmentDto(
                active.Id,
                active.Club?.Name ?? "Unknown Club",
                active.MembershipPlan?.Name ?? "Unknown Plan",
                active.TotalPrice,
                active.Amount,
                active.ExpiresAt,
                active.Source.ToString(),
                active.ExistingMembershipId
            )).ToList();

            return dtos;
        }
    }
}
