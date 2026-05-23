using EaseClub.Application.Features.Events.Dtos;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;

namespace EaseClub.Application.Features.Events.Commands.PublishEvent;

public record PublishEventCommand(Guid Id) : IRequest<Result<EventActionResponseDto>>;
