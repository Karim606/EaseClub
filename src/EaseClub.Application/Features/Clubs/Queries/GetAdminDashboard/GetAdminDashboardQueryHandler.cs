using EaseClub.Domain.Common.Results;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Clubs.Queries.GetAdminDashboard
{
    public class GetAdminDashboardQueryHandler(IClubAdminDashboardQueryService queryService)
        : IRequestHandler<GetAdminDashboardQuery, Result<ClubAdminDashboardResponse>>
    {
        public async Task<Result<ClubAdminDashboardResponse>> Handle(GetAdminDashboardQuery request, CancellationToken cancellationToken)
        {
            return await queryService.GetDashboardSummaryAsync(request.ClubId, cancellationToken);
        }
    }
}
