using EaseClub.Application.Features.Events.Dtos;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;

namespace EaseClub.Application.Features.Events.Queries.GetRegistrationById;

public record GetRegistrationByIdQuery(Guid RegistrationId) : IRequest<Result<EventRegistrationDto>>;
