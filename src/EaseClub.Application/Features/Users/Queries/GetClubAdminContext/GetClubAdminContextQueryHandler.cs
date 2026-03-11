using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.ClubAdmin;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Users.Queries.GetClubAdminContext
{
    public class GetClubAdminContextQueryHandler(
        IClubAdminUserRepository clubAdminUserRepo,
        ICurrentUserService currentUserService) : IRequestHandler<GetClubAdminContextQuery, Result<ClubAdminContextResponse>>
    {
        public async Task<Result<ClubAdminContextResponse>> Handle(GetClubAdminContextQuery request, CancellationToken cancellationToken)
        {
            var res = Guid.TryParse(currentUserService.GetId(), out var userId);

            if (!res) { return Error.Unauthorized(); }

            var admin = await clubAdminUserRepo.GetWithClubAsync(userId, cancellationToken);

            if (admin == null) return Error.NotFound(description: "User is not an admin for any club.");

            return new  ClubAdminContextResponse(admin.ClubId, admin.Club.Name);
        }
    }
}
