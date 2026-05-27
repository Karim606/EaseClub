using EaseClub.Application.Features.Events.Dtos;
using EaseClub.Domain.Common.Results;
using EaseClub.Application.Common.Pagination;
using EaseClub.Domain.Events.Enums;
using MediatR;
using System;

namespace EaseClub.Application.Features.Events.Queries;

public record GetClubEventsQuery(
    Guid ClubId,
    string? Search,
    EventStatus? Status,
    PaginationRequest Pagination
) : IRequest<Result<UnifiedPaginatedResponse<EventSummaryDto>>>;
