using EaseClub.Application.Features.Events.Dtos;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Memberships;
using MediatR;
using System;
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
        var memberships = await membershipRepository.GetByMemberIdAsync(request.UserId,null,cancellationToken);
        var membershipsInClub = memberships.Where(m => m.ClubId == request.ClubId);


        List<FamilyMemberDto> dtos = new List<FamilyMemberDto>();
        foreach (var fm in membershipsInClub){
            dtos.AddRange(fm.FamilyMembers.Select(fm => new FamilyMemberDto(
            fm.Id,
            fm.FullName,
            fm.Relationship,
            fm.GetAge(DateOnly.FromDateTime(DateTime.UtcNow))
            )).ToList());
        }
        return dtos;
    }
}
