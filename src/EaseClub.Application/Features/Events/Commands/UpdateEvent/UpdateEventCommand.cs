using EaseClub.Application.Features.Events.Dtos;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Events.Enums;
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
    EventAccessType AccessType,
    string Venue = "",
    Guid? ImageId = null,
    string? Badge = null) : IRequest<Result<EventActionResponseDto>>;
