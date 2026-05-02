using EaseClub.Application.Features.Events.Dtos;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Events;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Events.Queries;

public class GetEventByIdQueryHandler(IEventRepository eventRepository)
    : IRequestHandler<GetEventByIdQuery, Result<EventDto>>
{
    public async Task<Result<EventDto>> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetWithDetailsAsync(request.Id, cancellationToken);
        if (@event == null)
        {
            return Error.NotFound("Event.NotFound", "Event not found.");
        }

        var dto = new EventDto(
            @event.Id,
            @event.ClubId,
            @event.Name,
            @event.Description,
            @event.StartDate,
            @event.EndDate,
            @event.Capacity,
            @event.Audience,
            @event.Status,
            @event.Venue,
            @event.ImageUrl,
            @event.Badge,
            @event.IsFeatured,
            @event.TicketTypes.Select(t => new TicketTypeDto(
                t.Id,
                t.Name,
                t.Description,
                t.Category,
                t.Price,
                t.Quantity,
                t.MaxPerMember)).ToList()
        );

        return dto;
    }
}
