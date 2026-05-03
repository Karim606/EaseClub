using EaseClub.Application.Features.Events.Dtos;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;

namespace EaseClub.Application.Features.Events.Commands.RemoveTicketType;

public record RemoveTicketTypeCommand(Guid EventId, Guid TicketTypeId) : IRequest<Result<EventActionResponseDto>>;
