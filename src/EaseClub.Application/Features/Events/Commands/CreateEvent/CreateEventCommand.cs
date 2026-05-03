using EaseClub.Application.Features.Events.Dtos;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Events.Enums;
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
    Audience Audience,
    string Venue = "",
    string? ImageUrl = null,
    string? Badge = null,
    bool IsFeatured = false) : IRequest<Result<EventActionResponseDto>>;
