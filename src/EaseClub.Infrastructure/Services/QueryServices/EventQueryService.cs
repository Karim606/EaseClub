using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Common.Pagination;
using EaseClub.Application.Common.Pagination.Results;
using EaseClub.Application.Features.Events.Dtos;
using EaseClub.Application.Features.Events.Queries;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Events;
using EaseClub.Domain.Events.Entities;
using EaseClub.Domain.Events.Enums;
using EaseClub.Domain.Memberships;
using EaseClub.Infrastructure.Common.QueryServices;
using EaseClub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Services.QueryServices
{
    public class EventQueryService : BaseQueryService<Event>, IEventQueryService
    {
        private readonly IFileStorageService _fileStorageService;

        public EventQueryService(
            AppDbContext context,
            ILogger<EventQueryService> logger,
            IFileStorageService fileStorageService) : base(context, logger)
        {
            _fileStorageService = fileStorageService;
        }

        public async Task<Result<UnifiedPaginatedResponse<EventSummaryDto>>> GetClubEventsAsync(
            Guid clubId, string? search, EventStatus? status, PaginationRequest parameters, CancellationToken ct)
        {
            var query = Query().Where(e => e.ClubId == clubId);

            // 1. Status Filter
            if (status.HasValue)
            {
                query = query.Where(e => e.Status == status.Value);
            }

            // 2. Search by Event Name
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(e => e.Name.Contains(search));
            }

            return await GetUnifiedPaginatedAsync<EventSummaryDto, DateTime>(
                query,
                parameters,
                selector: e => new EventSummaryDto(
                    e.Id,
                    e.ClubId,
                    e.Name,
                    e.Description,
                    e.StartDate,
                    e.EndDate,
                    e.AccessType,
                    e.Status,
                    e.Venue,
                    _context.Clubs.Where(c => c.Id == e.ClubId).Select(c => c.Name).FirstOrDefault() ?? string.Empty,
                    e.Image != null ? _fileStorageService.GetFileUrl(e.Image.FilePath) : null,
                    e.Badge,
                    e.TicketTypes.Sum(t => t.TotalQuantity), // Capacity
                    e.TicketTypes.Sum(t => t.TotalQuantity - t.SoldQuantity), // Remaining Capacity
                    e.Registrations.Count(r => r.Status != RegistrationStatus.Cancelled), // Registrations Count
                    e.AccessType == EventAccessType.Public ? true : false
                ),
                orderSelector: e => e.StartDate,
                cancellationToken: ct
            );
        }

        public async Task<Result<UnifiedPaginatedResponse<EventSummaryDto>>> GetUpcomingEventsAsync(
            Guid? clubId, Guid? memberId, bool eligibleOnly, string? search, PaginationRequest parameters, CancellationToken ct)
        {
            // Fetch the user's active club memberships first to avoid EF Core referencing DbContext in projection
            var userClubIds = new List<Guid>();
            if (memberId.HasValue)
            {
                userClubIds = await _context.Memberships
                    .Where(m => m.MemberId == memberId.Value && m.Status == MembershipStatus.Active)
                    .Select(m => m.ClubId)
                    .ToListAsync(ct);
            }

            var query = Query()
                .Where(e => e.Status == EventStatus.Published && e.StartDate > DateTime.UtcNow);

            if (clubId.HasValue && clubId.Value != Guid.Empty)
            {
                query = query.Where(e => e.ClubId == clubId.Value);
            }

            if (eligibleOnly && memberId.HasValue)
            {
                query = query.Where(e => e.AccessType == EventAccessType.Public 
                                         || userClubIds.Contains(e.ClubId));
            }

            // Search by Event Name
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(e => e.Name.Contains(search));
            }

            return await GetUnifiedPaginatedAsync<EventSummaryDto, DateTime>(
                query,
                parameters,
                selector: e => new EventSummaryDto(
                    e.Id,
                    e.ClubId,
                    e.Name,
                    e.Description,
                    e.StartDate,
                    e.EndDate,
                    e.AccessType,
                    e.Status,
                    e.Venue,
                    _context.Clubs.Where(c => c.Id == e.ClubId).Select(c => c.Name).FirstOrDefault() ?? string.Empty,
                    e.Image != null ? _fileStorageService.GetFileUrl(e.Image.FilePath) : null,
                    e.Badge,
                    e.TicketTypes.Sum(t => t.TotalQuantity), // Capacity
                    e.TicketTypes.Sum(t => t.TotalQuantity - t.SoldQuantity), // Remaining Capacity
                    e.Registrations.Count(r => r.Status != RegistrationStatus.Cancelled),
                    (e.AccessType == EventAccessType.Public || userClubIds.Contains(e.ClubId)) ? true : false
                ),
                orderSelector: e => e.StartDate,
                cancellationToken: ct
            );
        }

        public async Task<Result<UnifiedPaginatedResponse<EventRegistrationDto>>> GetEventRegistrationsAsync(
            Guid eventId, PaginationRequest parameters, CancellationToken ct)
        {
            var query = _context.EventRegistrations
                .Where(r => r.EventId == eventId)
                .Include(r => r.Attendees);

            var helper = new RegistrationQueryHelper(_context, _logger, _fileStorageService);
            return await helper.GetRegistrationsAsync(query, parameters, ct);
        }

        // Nested helper class to paginated query EventRegistration (since EventQueryService inherits BaseQueryService<Event>)
        private class RegistrationQueryHelper : BaseQueryService<EventRegistration>
        {
            private readonly IFileStorageService _fileStorageService;

            public RegistrationQueryHelper(AppDbContext context, ILogger logger, IFileStorageService fileStorageService) : base(context, logger)
            {
                _fileStorageService = fileStorageService;
            }

            public async Task<Result<UnifiedPaginatedResponse<EventRegistrationDto>>> GetRegistrationsAsync(
                IQueryable<EventRegistration> query, PaginationRequest parameters, CancellationToken ct)
            {
                return await GetUnifiedPaginatedAsync<EventRegistrationDto, DateTime>(
                    query,
                    parameters,
                    selector: r => new EventRegistrationDto(
                        r.Id,
                        r.EventId,
                        r.Event.ClubId,
                        r.RegistrantId,
                        r.IsRegistrantAttending,
                        r.Status,
                        r.InvoiceId,
                        r.TotalBasePrice,
                        r.DiscountAmount,
                        r.FinalTotal,
                        r.AppliedPolicies,
                        r.ReadableId,
                        r.Event.Name,
                        r.Event.StartDate,
                        r.Event.Venue,
                        r.Event.Image != null ? _fileStorageService.GetFileUrl(r.Event.Image.FilePath) : null,
                        _context.Clubs.Where(c => c.Id == r.Event.ClubId).Select(c => c.Name).FirstOrDefault() ?? string.Empty,
                        r.Attendees.Select(a => new AttendeeDto(
                            a.Id,
                            a.TicketTypeId,
                            r.Event.TicketTypes.First(t => t.Id == a.TicketTypeId).Category,
                            a.AttendeeId,
                            a.AttendeeName,
                            a.Age,
                            a.Gender
                        )).ToList()
                    ),
                    orderSelector: r => r.CreatedAt,
                    cancellationToken: ct
                );
            }
        }
    }
}
