using EaseClub.Application.Features.Events.Dtos;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Memberships;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Events.Queries.GetFamilyMembersForEvent;

public class GetFamilyMembersForEventQueryHandler(IMembershipRepository membershipRepository)
    : IRequestHandler<GetFamilyMembersForEventQuery, Result<List<FamilyMemberDto>>>
{
    public async Task<Result<List<FamilyMemberDto>>> Handle(GetFamilyMembersForEventQuery request, CancellationToken cancellationToken)
    {
        var memberships = await membershipRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        var membership = memberships.FirstOrDefault(m => m.ClubId == request.ClubId);

        if (membership == null)
            return Error.NotFound("Membership.NotFound", "Membership not found in this club.");

        var dtos = membership.FamilyMembers.Select(fm => new FamilyMemberDto(
            fm.Id,
            fm.FullName,
            fm.Relationship,
            fm.GetAge(null)
        )).ToList();

        return dtos;
    }
}
