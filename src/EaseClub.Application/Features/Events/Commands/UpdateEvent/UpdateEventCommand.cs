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
    int Capacity) : IRequest<Result<Success>>;
