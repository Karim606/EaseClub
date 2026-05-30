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

using EaseClub.Application.Common.Interfaces;

namespace EaseClub.Infrastructure.Services.QueryServices
{
    public class ClubsQueryService : BaseQueryService<Club>, IClubsQueryService
    {
        private readonly IFileStorageService _fileStorageService;

        public ClubsQueryService(AppDbContext context, ILogger<ClubsQueryService> logger, IFileStorageService fileStorageService) : base(context, logger)
        {
            _fileStorageService = fileStorageService;
        }

        public async Task<Result<CursorPaginatedResult<ClubsDto>>> GetClubsAsync(CursorPaginationParameters parameters, CancellationToken ct)
        {
            var query = Query();

            var result = await GetPaginatedAsync<ClubsDto, string, CursorPaginatedResult<ClubsDto>>(
                query,
                parameters,
                selector: c => new ClubsDto(c.Id, c.Name, c.Logo != null ? c.Logo.FilePath : null, c.CoverImage != null ? c.CoverImage.FilePath : null),
                orderSelector: c => c.Name,
                ct);

            if (result.IsSuccess && result.Value.Items.Any())
            {
                foreach (var item in result.Value.Items)
                {
                    if (!string.IsNullOrEmpty(item.LogoUrl))
                    {
                        item.LogoUrl = _fileStorageService.GetFileUrl(item.LogoUrl);
                    }
                    if (!string.IsNullOrEmpty(item.CoverImageUrl))
                    {
                        item.CoverImageUrl = _fileStorageService.GetFileUrl(item.CoverImageUrl);
                    }
                }
            }

            return result;
        }
    }
}
