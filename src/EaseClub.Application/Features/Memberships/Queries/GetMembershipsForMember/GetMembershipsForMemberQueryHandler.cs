using Microsoft.Extensions.Logging;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Memberships;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Memberships.Queries.GetMembershipsForMember
{
    public class GetMembershipsForMemberQueryHandler(IMembershipRepository membershipRepository,ICurrentUserService currentUserService,
        ILogger<GetMembershipsForMemberQueryHandler> logger) : IRequestHandler<GetMembershipsForMemberQuery, Result<List<MembershipForMemberDto>>>
    {
        public async Task<Result<List<MembershipForMemberDto>>> Handle(GetMembershipsForMemberQuery request, CancellationToken cancellationToken)
        {
          

            var memberships = await  membershipRepository.GetByMemberIdAsync(request.userId,request.Status);

            return memberships.Select( x => new MembershipForMemberDto(x.Id,x.MembershipNumber,x.Club.Id,x.Club.Name,x.Status,x.GetCurrentCycle().Period)
            ).ToList();
        }
    }
}
