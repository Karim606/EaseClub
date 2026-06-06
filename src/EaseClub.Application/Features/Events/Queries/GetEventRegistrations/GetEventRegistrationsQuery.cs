using EaseClub.Application.Features.Events.Dtos;
using EaseClub.Domain.Common.Results;
using EaseClub.Application.Common.Pagination;
using MediatR;
using System;

namespace EaseClub.Application.Features.Events.Queries.GetEventRegistrations;

public record GetEventRegistrationsQuery(
    Guid EventId,
    PaginationRequest Pagination
) : IRequest<Result<UnifiedPaginatedResponse<EventRegistrationDto>>>;
