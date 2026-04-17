using EaseClub.Domain.Common.Results;
using MediatR;
using System;

namespace EaseClub.Application.Features.Events.Commands.CancelEvent;

public record CancelEventCommand(Guid Id) : IRequest<Result<Success>>;
