using EaseClub.Application.Features.Events.Dtos;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Events.ValueObjects;
using MediatR;
using System;
using System.Collections.Generic;

namespace EaseClub.Application.Features.Events.Commands.RegisterForEvent;

public record RegisterForEventCommand(
    Guid EventId,
    Guid RegistrantId,
    bool IsRegistrantAttending,
    string RegistrantName,
    int? RegistrantAge = null,
    string? RegistrantGender = null,
    List<AttendeeRequest>? Attendees = null) : IRequest<Result<EventActionResponseDto>>;
