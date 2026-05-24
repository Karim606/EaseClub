using EaseClub.Application.Features.Clubs.Queries;
using EaseClub.Application.Features.Clubs.Queries.GetAdminDashboard;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipApplications.Enums;
using EaseClub.Domain.Events.Enums;
using EaseClub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Services.QueryServices
{
    public class ClubAdminDashboardQueryService(AppDbContext context, ILogger<ClubAdminDashboardQueryService> logger)
        : IClubAdminDashboardQueryService
    {
        public async Task<Result<ClubAdminDashboardResponse>> GetDashboardSummaryAsync(Guid clubId, CancellationToken ct)
        {
            try
            {
                var now = DateTime.UtcNow;

                // 1. Fetch Metrics
                var pendingAppsCount = await context.MembershipApplications
                    .CountAsync(a => a.ClubId == clubId && a.Status == ApplicationStatus.Submitted, ct);

                var upcomingEventsCount = await context.Events
                    .CountAsync(e => e.ClubId == clubId && e.Status == EventStatus.Published && e.StartDate > now, ct);

                var branchesCount = await context.Branches
                    .CountAsync(b => b.ClubId == clubId, ct);

                var metrics = new ClubAdminDashboardMetrics(pendingAppsCount, upcomingEventsCount, branchesCount);

                // 2. Fetch Latest 5 Pending Applications
                var pendingApps = await context.MembershipApplications
                    .Where(a => a.ClubId == clubId && a.Status == ApplicationStatus.Submitted)
                    .OrderByDescending(a => a.SubmittedAt)
                    .Take(5)
                    .Select(a => new PendingApplicationDto(
                        a.Id,
                        a.Member.FirstName + " " + a.Member.LastName,
                        a.MembershipPlan.Name,
                        a.SubmittedAt
                    ))
                    .ToListAsync(ct);

                // 3. Fetch Top 5 Upcoming Events with Registration Fill Percentage
                var eventsList = await context.Events
                    .Where(e => e.ClubId == clubId && e.Status == EventStatus.Published && e.StartDate > now)
                    .OrderBy(e => e.StartDate)
                    .Take(5)
                    .Select(e => new
                    {
                        e.Id,
                        e.Name,
                        e.StartDate,
                        e.Capacity,
                        // Sum sold quantity from all ticket types
                        SoldTicketsCount = e.TicketTypes.Sum(t => t.SoldQuantity)
                    })
                    .ToListAsync(ct);

                var eventRegistrations = eventsList.Select(e => new DashboardEventRegistrationDto(
                    e.Id,
                    e.Name,
                    e.StartDate,
                    e.SoldTicketsCount,
                    e.Capacity,
                    e.Capacity > 0 ? Math.Round((double)e.SoldTicketsCount / e.Capacity * 100, 2) : 0
                )).ToList();

                return new ClubAdminDashboardResponse(clubId, metrics, pendingApps, eventRegistrations);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching dashboard summary for club {ClubId}", clubId);
                return Error.Failure("Dashboard.FetchFailed", "An error occurred while loading the dashboard.");
            }
        }
    }
}
