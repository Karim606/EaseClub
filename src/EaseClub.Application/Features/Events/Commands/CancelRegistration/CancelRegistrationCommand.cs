using EaseClub.Application.Features.Events.Dtos;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;

namespace EaseClub.Application.Features.Events.Commands.CancelRegistration;

public record CancelRegistrationCommand(Guid EventId, Guid RegistrationId) : IRequest<Result<EventActionResponseDto>>;
