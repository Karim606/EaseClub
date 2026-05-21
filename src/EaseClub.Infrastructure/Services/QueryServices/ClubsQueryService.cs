using EaseClub.Application.Common.Pagination.Parameters;
using EaseClub.Application.Common.Pagination.Results;
using EaseClub.Application.Features.Clubs.Queries;
using EaseClub.Application.Features.Clubs.Queries.GetClubs;
using EaseClub.Domain.Clubs;
using EaseClub.Domain.Common.Results;
using EaseClub.Infrastructure.Common.QueryServices;
using EaseClub.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Services.QueryServices
{
    public class ClubsQueryService : BaseQueryService<Club>, IClubsQueryService
    {
        public ClubsQueryService(AppDbContext context, ILogger<ClubsQueryService> logger) : base(context, logger)
        {
        }

        public async Task<Result<CursorPaginatedResult<ClubsDto>>> GetClubsAsync(CursorPaginationParameters parameters, CancellationToken ct)
        {
            var query = Query();

           return await GetPaginatedAsync<ClubsDto,string, CursorPaginatedResult<ClubsDto>>(query, parameters, selector: c => new ClubsDto(c.Id, c.Name,c.LogoUrl), orderSelector: c => c.Name, ct);
        }
    }
}
