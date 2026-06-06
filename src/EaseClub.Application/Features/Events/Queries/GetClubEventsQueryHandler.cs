using EaseClub.Application.Features.Events.Dtos;
using EaseClub.Domain.Common.Results;
using EaseClub.Application.Common.Pagination;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Events.Queries;

public class GetClubEventsQueryHandler(IEventQueryService eventQueryService)
    : IRequestHandler<GetClubEventsQuery, Result<UnifiedPaginatedResponse<EventSummaryDto>>>
{
    public async Task<Result<UnifiedPaginatedResponse<EventSummaryDto>>> Handle(GetClubEventsQuery request, CancellationToken cancellationToken)
    {
        return await eventQueryService.GetClubEventsAsync(
            request.ClubId, 
            request.Search, 
            request.Status, 
            request.Pagination, 
            cancellationToken);
    }
}
