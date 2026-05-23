using EaseClub.Application.Features.Events.Dtos;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;

namespace EaseClub.Application.Features.Events.Queries;

public record GetEventByIdQuery(Guid Id) : IRequest<Result<EventDto>>;
