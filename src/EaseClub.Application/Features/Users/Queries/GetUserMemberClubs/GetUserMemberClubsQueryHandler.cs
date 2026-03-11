using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Memberships;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Users.Queries.GetUserMemberClubs
{
    public class GetUserMemberClubsQueryHandler(
        IMembershipRepository membershipRepository,
        ICurrentUserService currentUserService) : IRequestHandler<GetUserMemberClubsQuery, Result<List<UserClubResponse>>>
    {
        public async Task<Result<List<UserClubResponse>>> Handle(GetUserMemberClubsQuery request, CancellationToken cancellationToken)
        {
            var res = Guid.TryParse(currentUserService.GetId(), out var userId);
            if (!res) return Error.Unauthorized();
            
            var clubs = await  membershipRepository.GetByUserIdAsync(userId, cancellationToken);

            return clubs.Select(m => new UserClubResponse(m.ClubId, m.Club.Name)).ToList();
        }
    }
}
