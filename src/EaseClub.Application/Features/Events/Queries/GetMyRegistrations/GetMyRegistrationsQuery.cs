using EaseClub.Application.Features.Events.Dtos;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;

namespace EaseClub.Application.Features.Events.Queries.GetMyRegistrations;

public record GetMyRegistrationsQuery(Guid UserId) : IRequest<Result<List<EventRegistrationDto>>>;
