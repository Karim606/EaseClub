using EaseClub.Domain.Common.Results;
using MediatR;
using System;

namespace EaseClub.Application.Features.Events.Commands.PublishEvent;

public record PublishEventCommand(Guid Id) : IRequest<Result<Success>>;
