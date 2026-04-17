using EaseClub.Application.Features.Events.Dtos;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Events;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Events.Queries;

public class GetClubEventsQueryHandler(IEventRepository eventRepository)
    : IRequestHandler<GetClubEventsQuery, Result<List<EventSummaryDto>>>
{
    public async Task<Result<List<EventSummaryDto>>> Handle(GetClubEventsQuery request, CancellationToken cancellationToken)
    {
        var events = await eventRepository.GetByClubIdAsync(request.ClubId, cancellationToken);
        
        var dtos = events.Select(e => new EventSummaryDto(
            e.Id,
            e.Name,
            e.StartDate,
            e.EndDate,
            e.Status,
            e.Registrations.Count
        )).ToList();

        return dtos;
    }
}
