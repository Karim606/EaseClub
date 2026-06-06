using EaseClub.Application.Features.Events.Dtos;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;

namespace EaseClub.Application.Features.Events.Commands.CreateEvent;

public record CreateEventCommand(
    Guid ClubId,
    string Name,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    int Capacity,
    string Venue = "",
    Guid? ImageId = null,
    string? Badge = null) : IRequest<Result<EventActionResponseDto>>;
