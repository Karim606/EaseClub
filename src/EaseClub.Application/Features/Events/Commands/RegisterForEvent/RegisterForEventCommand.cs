using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Events.ValueObjects;
using MediatR;
using System;
using System.Collections.Generic;

namespace EaseClub.Application.Features.Events.Commands.RegisterForEvent;

public record RegisterForEventCommand(
    Guid EventId,
    Guid RegistrantId,
    List<AttendeeRequest> Attendees) : IRequest<Result<Guid>>;
