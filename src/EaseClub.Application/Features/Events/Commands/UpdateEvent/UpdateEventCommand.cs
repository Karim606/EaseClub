using EaseClub.Application.Features.Events.Dtos;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;

namespace EaseClub.Application.Features.Events.Commands.UpdateEvent;

public record UpdateEventCommand(
    Guid Id,
    string Name,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    int Capacity,
    string Venue = "",
    string? ImageUrl = null,
    string? Badge = null,
    bool IsFeatured = false) : IRequest<Result<EventActionResponseDto>>;
