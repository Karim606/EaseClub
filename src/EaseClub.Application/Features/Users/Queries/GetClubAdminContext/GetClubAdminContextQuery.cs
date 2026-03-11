using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Users.Queries.GetClubAdminContext
{
    public record GetClubAdminContextQuery() : IRequest<Result<ClubAdminContextResponse>>;

    public record ClubAdminContextResponse(Guid ManagedClubId, string ClubName);
}
