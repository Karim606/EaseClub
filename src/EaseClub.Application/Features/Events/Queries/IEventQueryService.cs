using EaseClub.Application.Common.Pagination;
using EaseClub.Application.Features.Events.Dtos;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Events.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Events.Queries
{
    public interface IEventQueryService
    {
        Task<Result<UnifiedPaginatedResponse<EventSummaryDto>>> GetClubEventsAsync(
            Guid clubId,
            string? search,
            EventStatus? status,
            PaginationRequest parameters,
            CancellationToken ct);

        Task<Result<UnifiedPaginatedResponse<EventSummaryDto>>> GetUpcomingEventsAsync(
            Guid clubId,
            string? search,
            PaginationRequest parameters,
            CancellationToken ct);

        Task<Result<UnifiedPaginatedResponse<EventRegistrationDto>>> GetEventRegistrationsAsync(
            Guid eventId,
            PaginationRequest parameters,
            CancellationToken ct);
    }
}
