using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Events.Enums;
using MediatR;
using System;

namespace EaseClub.Application.Features.Events.Commands.AddTicketType;

public record AddTicketTypeCommand(
    Guid EventId,
    string Name,
    string Description,
    AttendeeCategory Category,
    decimal Price,
    int Quantity,
    int? MaxPerMember = null) : IRequest<Result<Guid>>;
