using EaseClub.Application.Features.Events.Dtos;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;

namespace EaseClub.Application.Features.Events.Commands.UpdateTicketType;

public record UpdateTicketTypeCommand(
    Guid EventId,
    Guid TicketTypeId,
    decimal BasePrice,
    int TotalQuantity,
    int? MaxPerMember = null,
    int? MinAge = null,
    int? MaxAge = null,
    string? GenderRestriction = null) : IRequest<Result<EventActionResponseDto>>;
