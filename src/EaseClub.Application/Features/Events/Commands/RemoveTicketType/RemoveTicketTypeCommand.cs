using EaseClub.Domain.Common.Results;
using MediatR;
using System;

namespace EaseClub.Application.Features.Events.Commands.RemoveTicketType;

public record RemoveTicketTypeCommand(Guid EventId, Guid TicketTypeId) : IRequest<Result<Success>>;
