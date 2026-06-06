using EaseClub.Application.Features.Events.Dtos;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Events;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Events.Queries.GetEventStats
{
    public class GetEventStatsQueryHandler(IEventRepository eventRepository)
        : IRequestHandler<GetEventStatsQuery, Result<EventStatsDto>>
    {
        public async Task<Result<EventStatsDto>> Handle(GetEventStatsQuery request, CancellationToken cancellationToken)
        {
            var @event = await eventRepository.GetWithDetailsAsync(request.EventId, cancellationToken);
            if (@event == null)
                return Error.NotFound("Event.NotFound", "Event not found.");

            var stats = @event.GetAttendanceStats();

            var detailsDictionary = stats.Details
                .ToDictionary(k => k.Key.ToString(), v => v.Value);

            return new EventStatsDto(
                stats.TotalCapacity,
                stats.TotalSold,
                stats.TotalAvailable,
                detailsDictionary
            );
        }
    }
}
