using System;
using System.Collections.Generic;

namespace EaseClub.Application.Features.Clubs.Queries.GetAdminDashboard
{
    public record ClubAdminDashboardResponse(
        Guid ClubId,
        ClubAdminDashboardMetrics Metrics,
        List<PendingApplicationDto> PendingApplications,
        List<DashboardEventRegistrationDto> EventRegistrations
    );

    public record ClubAdminDashboardMetrics(
        int PendingApplicationsCount,
        int UpcomingEventsCount,
        int BranchesCount
    );

    public record PendingApplicationDto(
        Guid ApplicationId,
        string ApplicantName,
        string PlanName,
        DateTime? SubmittedAt
    );

    public record DashboardEventRegistrationDto(
        Guid EventId,
        string EventTitle,
        DateTime StartDate,
        int SoldTickets,
        int MaxCapacity,
        double FillPercentage
    );
}
