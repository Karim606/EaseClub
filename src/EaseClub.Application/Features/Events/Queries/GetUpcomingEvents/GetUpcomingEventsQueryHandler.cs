using EaseClub.Application.Features.Events.Dtos;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Events;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Events.Queries.GetUpcomingEvents;

public class GetUpcomingEventsQueryHandler(IEventRepository eventRepository)
    : IRequestHandler<GetUpcomingEventsQuery, Result<List<EventSummaryDto>>>
{
    public async Task<Result<List<EventSummaryDto>>> Handle(GetUpcomingEventsQuery request, CancellationToken cancellationToken)
    {
        var events = await eventRepository.GetUpcomingEventsAsync(request.ClubId, cancellationToken);
        
        var dtos = events.Select(e => new EventSummaryDto(
            e.Id,
            e.Name,
            e.StartDate,
            e.EndDate,
            e.Status,
            e.Venue,
            e.ImageUrl,
            e.Badge,
            e.IsFeatured,
            e.Registrations.Count
        )).ToList();

        return dtos;
    }
}
