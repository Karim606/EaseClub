using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Users.Queries.GetUserMemberClubs
{
    public record GetUserMemberClubsQuery() : IRequest<Result<List<UserClubResponse>>>;

    public record UserClubResponse(Guid ClubId, string ClubName);
}
