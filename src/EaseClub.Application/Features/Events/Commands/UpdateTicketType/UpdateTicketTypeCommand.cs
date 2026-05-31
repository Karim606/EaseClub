using EaseClub.Application.Features.Events.Dtos;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Events.Enums;
using MediatR;
using System;

namespace EaseClub.Application.Features.Events.Commands.UpdateTicketType;

public record UpdateTicketTypeCommand(
    Guid EventId,
    Guid TicketTypeId,
    AttendeeCategory Category,
    decimal BasePrice,
    int TotalQuantity,
    int? MaxPerMember = null,
    int? MinAge = null,
    int? MaxAge = null,
    string? GenderRestriction = null) : IRequest<Result<EventActionResponseDto>>;
