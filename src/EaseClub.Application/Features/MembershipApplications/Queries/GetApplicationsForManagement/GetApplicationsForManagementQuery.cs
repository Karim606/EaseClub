using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Common.Pagination.Parameters;
using EaseClub.Application.Common.Pagination.Results;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipApplications.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipApplications.Queries.GetApplicationsForManagement
{
    public record GetApplicationsForManagementQuery([FromRoute]Guid ClubId,GetApplicationsQueryFilters filters,OffsetPaginationParameters pagination):IRequest<Result<OffsetPaginatedResult<MembershipAppAdminDto>>>,IRequireClubAdmin;

    public class GetApplicationsQueryFilters {
        public AppStatus? Status { get; set; }
        public DateOnly? SubmittedFrom { get; set; }
        public DateOnly? SubmittedTo { get; set; }
        public string? TrackingNumber { get; set; }

    }

    public enum AppStatus
    {

        Submitted,     // Pending Review
        //UnderReview,   // Admin is looking at it
        NeedsChanges,  // Admin sent it back to the user
        Approved,      // The decision is made!
        Rejected       // The decision is made!
    }

    public class MembershipAppAdminDto
    {
        public Guid Id { get; set; }
        public Guid ClubId { get; set; }
        public Guid UserId { get; set; }
        public string TrackingNumber { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }

        public string MembershipType { get; set; }
        public string MembershipPlanName { get; set; }
        public DateTime SubmittedAt { get; set; }

        public ApplicationStatus Status { get; set; } //Status

    }
}
