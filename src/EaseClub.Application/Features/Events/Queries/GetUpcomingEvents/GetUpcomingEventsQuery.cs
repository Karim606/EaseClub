using EaseClub.Application.Features.Events.Dtos;
using EaseClub.Domain.Common.Results;
using EaseClub.Application.Common.Pagination;
using MediatR;
using System;

namespace EaseClub.Application.Features.Events.Queries.GetUpcomingEvents;

public record GetUpcomingEventsQuery(
    Guid ClubId,
    string? Search,
    PaginationRequest Pagination
) : IRequest<Result<UnifiedPaginatedResponse<EventSummaryDto>>>;
