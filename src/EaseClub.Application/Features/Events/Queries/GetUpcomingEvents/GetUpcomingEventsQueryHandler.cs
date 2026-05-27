using EaseClub.Application.Features.Events.Dtos;
using EaseClub.Domain.Common.Results;
using EaseClub.Application.Common.Pagination;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Events.Queries.GetUpcomingEvents;

public class GetUpcomingEventsQueryHandler(IEventQueryService eventQueryService)
    : IRequestHandler<GetUpcomingEventsQuery, Result<UnifiedPaginatedResponse<EventSummaryDto>>>
{
    public async Task<Result<UnifiedPaginatedResponse<EventSummaryDto>>> Handle(GetUpcomingEventsQuery request, CancellationToken cancellationToken)
    {
        return await eventQueryService.GetUpcomingEventsAsync(
            request.ClubId, 
            request.Search, 
            request.Pagination, 
            cancellationToken);
    }
}
