using EaseClub.Application.Features.Clubs.Queries.GetAdminDashboard;
using EaseClub.Domain.Common.Results;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Clubs.Queries
{
    public interface IClubAdminDashboardQueryService
    {
        Task<Result<ClubAdminDashboardResponse>> GetDashboardSummaryAsync(Guid clubId, CancellationToken ct);
    }
}
