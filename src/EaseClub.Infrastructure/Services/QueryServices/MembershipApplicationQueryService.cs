using EaseClub.Application.Common.Pagination;
using EaseClub.Application.Common.Pagination.Parameters;
using EaseClub.Application.Common.Pagination.Results;
using EaseClub.Application.Features.MembershipApplications;
using EaseClub.Application.Features.MembershipApplications.Queries;
using EaseClub.Application.Features.MembershipApplications.Queries.GetApplicationsForManagement;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipApplications;
using EaseClub.Domain.MembershipApplications.Enums;
using EaseClub.Domain.Memberships;
using EaseClub.Infrastructure.Common.QueryServices;
using EaseClub.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace EaseClub.Infrastructure.Services.QueryServices
{
    public class MembershipApplicationQueryService : BaseQueryService<MembershipApplication>, IMembershipApplicationQueryService
    {
        public MembershipApplicationQueryService(AppDbContext context, ILogger<MembershipApplicationQueryService> logger) : base(context, logger)
        {
        }

        public async Task<Result<OffsetPaginatedResult<MembershipAppAdminDto>>> GetMembershipApplicationsForManagementAsync(Guid clubId, GetApplicationsQueryFilters filters, OffsetPaginationParameters parameters, CancellationToken ct = default)
        {
            var query = Query().Where(p => p.ClubId == clubId);

            if (filters != null)
            {
                if (!string.IsNullOrWhiteSpace(filters.TrackingNumber))
                    query = query.Where(p => p.TrackingNumber.ToLower() == filters.TrackingNumber.ToLower());

                if (filters.Status.HasValue)
                {
                    var status = (ApplicationStatus)filters.Status.Value;
                    query = query.Where(p => p.Status == status);
                }

                var from = filters.SubmittedFrom?.ToDateTime(TimeOnly.MinValue);
                var to = filters.SubmittedTo?.ToDateTime(TimeOnly.MaxValue);

                if (from.HasValue && to.HasValue) query = query.Where(p => p.CreatedAt >= from.Value && p.CreatedAt <= to.Value);
                else if (from.HasValue) query = query.Where(p => p.CreatedAt >= from.Value);
                else if (to.HasValue) query = query.Where(p => p.CreatedAt <= to.Value);
            }

            return await GetPaginatedAsync<MembershipAppAdminDto, DateTime, OffsetPaginatedResult<MembershipAppAdminDto>>(
                query,
                parameters,
                selector: p => new MembershipAppAdminDto()
                {
                    Id = p.Id,
                    ClubId = p.ClubId,
                    UserId = p.MemberId,
                    TrackingNumber = p.TrackingNumber,
                    MembershipType = p.MembershipType.Name,
                    Email = p.Member.Email.Value,
                    UserName = p.Member.FirstName + " " + p.Member.LastName,
                    MembershipPlanName = p.MembershipPlan.Name,
                    SubmittedAt = p.SubmittedAt,
                    ReviewedAt = p.Reviews.OrderByDescending(r => r.Date).Select(r => (DateTime?)r.Date).FirstOrDefault(),
                    LatestDecision = p.Reviews.OrderByDescending(r => r.Date).Select(r => r.Decision.ToString()).FirstOrDefault(),
                    Status = p.Status,
                },
                orderSelector: p => p.CreatedAt,
                ct);
        }

        public async Task<Result<UnifiedPaginatedResponse<MembershipAppDto>>> GetMembershipApplicationSummaryAsync(Guid? clubId,
            Guid? userId, ApplicationStatus? status, PaginationRequest parameters, CancellationToken ct = default)
        {
            var query = Query();
            if (clubId != null)
            {
                query = query.Where(p => p.ClubId == clubId && p.MemberId == userId);
            }
            if (userId != null)
            {
                query = query.Where(p => p.MemberId == userId);
            }

            if (status != null)
            {
                query = query.Where(p => p.Status == status);
            }

            return await GetUnifiedPaginatedAsync<MembershipAppDto, DateTime>(
                query,
                parameters,
                selector: p => new MembershipAppDto()
                {
                    ClubId = p.ClubId,
                    UserId = p.MemberId,
                    TrackingNumber = p.TrackingNumber,
                    MembershipType = p.MembershipType.Name,
                    MembershipPlanName = p.MembershipPlan.Name,
                    SubmittedAt = p.SubmittedAt,
                    Status = p.Status,
                    EnrollmentInvoiceId = _context.Enrollments
                        .Where(pe => pe.MembershipApplicationId == p.Id && pe.Status == EnrollmentStatus.WaitingForFirstPayment)
                        .Select(pe => pe.FirstInvoiceId)
                        .FirstOrDefault()
                },
                orderSelector: p => p.CreatedAt,
                ct);
        }
    }
}
