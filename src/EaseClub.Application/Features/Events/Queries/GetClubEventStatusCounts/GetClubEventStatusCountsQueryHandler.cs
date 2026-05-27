using EaseClub.Application.Features.Events.Dtos;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Events;
using EaseClub.Domain.Events.Enums;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Events.Queries.GetClubEventStatusCounts
{
    public class GetClubEventStatusCountsQueryHandler(IEventRepository eventRepository)
        : IRequestHandler<GetClubEventStatusCountsQuery, Result<ClubEventStatusCountsDto>>
    {
        public async Task<Result<ClubEventStatusCountsDto>> Handle(GetClubEventStatusCountsQuery request, CancellationToken cancellationToken)
        {
            var events = await eventRepository.GetByClubIdAsync(request.ClubId, cancellationToken);

            var total = events.Count;
            var published = events.Count(e => e.Status == EventStatus.Published);
            var draft = events.Count(e => e.Status == EventStatus.Draft);
            var cancelled = events.Count(e => e.Status == EventStatus.Cancelled);

            return new ClubEventStatusCountsDto(total, published, draft, cancelled);
        }
    }
}
