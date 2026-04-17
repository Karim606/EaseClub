using EaseClub.Domain.Common.Results;
using MediatR;
using System;

namespace EaseClub.Application.Features.Events.Commands.UpdateTicketType;

public record UpdateTicketTypeCommand(
    Guid EventId,
    Guid TicketTypeId,
    string Name,
    string Description,
    decimal Price,
    int Quantity,
    int? MaxPerMember = null) : IRequest<Result<Success>>;
