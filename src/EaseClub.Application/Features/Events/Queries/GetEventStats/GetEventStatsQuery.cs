using EaseClub.Application.Features.Events.Dtos;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;

namespace EaseClub.Application.Features.Events.Queries.GetEventStats
{
    public record GetEventStatsQuery(Guid EventId) : IRequest<Result<EventStatsDto>>;
}
