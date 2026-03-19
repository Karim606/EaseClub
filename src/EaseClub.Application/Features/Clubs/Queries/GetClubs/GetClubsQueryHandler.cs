using EaseClub.Application.Common.Pagination.Results;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Clubs.Queries.GetClubs
{
    public class GetClubsQueryHandler(IClubsQueryService clubsQueryService) : IRequestHandler<GetClubsQuery, Result<CursorPaginatedResult<ClubsDto>>>
    {
        public async Task<Result<CursorPaginatedResult<ClubsDto>>> Handle(GetClubsQuery request, CancellationToken cancellationToken)
        {
            return await clubsQueryService.GetClubsAsync(request.parameters,cancellationToken);
        }
    }


}
