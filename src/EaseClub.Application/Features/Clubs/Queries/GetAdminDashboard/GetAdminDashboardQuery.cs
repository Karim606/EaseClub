using EaseClub.Domain.Common.Results;
using MediatR;
using System;

namespace EaseClub.Application.Features.Clubs.Queries.GetAdminDashboard
{
    public record GetAdminDashboardQuery(Guid ClubId) : IRequest<Result<ClubAdminDashboardResponse>>;
}
