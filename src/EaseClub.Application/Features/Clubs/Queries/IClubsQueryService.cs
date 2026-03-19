using EaseClub.Application.Common.Pagination.Parameters;
using EaseClub.Application.Common.Pagination.Results;
using EaseClub.Application.Features.Clubs.Queries.GetClubs;
using EaseClub.Domain.Common.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Clubs.Queries
{
    public interface IClubsQueryService
    {
        public Task<Result<CursorPaginatedResult<ClubsDto>>> GetClubsAsync(CursorPaginationParameters parameters,CancellationToken ct);
    }
}
