using EaseClub.Application.Features.Events.Dtos;
using EaseClub.Domain.Common.Results;
using EaseClub.Application.Common.Pagination;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Events.Queries.GetEventRegistrations;

public class GetEventRegistrationsQueryHandler(IEventQueryService eventQueryService)
    : IRequestHandler<GetEventRegistrationsQuery, Result<UnifiedPaginatedResponse<EventRegistrationDto>>>
{
    public async Task<Result<UnifiedPaginatedResponse<EventRegistrationDto>>> Handle(GetEventRegistrationsQuery request, CancellationToken cancellationToken)
    {
        return await eventQueryService.GetEventRegistrationsAsync(
            request.EventId,
            request.Pagination,
            cancellationToken);
    }
}
