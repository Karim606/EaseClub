using EaseClub.Application.Features.Events.Dtos;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Events;
using EaseClub.Domain.Clubs;
using EaseClub.Application.Common.Interfaces;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Events.Queries;

public class GetEventByIdQueryHandler(IEventRepository eventRepository, IClubRepository clubRepository, IFileStorageService fileStorageService)
    : IRequestHandler<GetEventByIdQuery, Result<EventDto>>
{
    public async Task<Result<EventDto>> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetWithDetailsAsync(request.Id, cancellationToken);
        if (@event == null)
        {
            return Error.NotFound("Event.NotFound", "Event not found.");
        }

        var club = await clubRepository.GetByIdAsync(@event.ClubId, cancellationToken);
        var clubName = club?.Name ?? "Unknown Club";

        var dto = new EventDto(
            @event.Id,
            @event.ClubId,
            clubName,
            @event.Name,
            @event.Description,
            @event.StartDate,
            @event.EndDate,
            @event.TicketTypes.Any() ? @event.TicketTypes.Sum(t => t.TotalQuantity) : @event.Capacity,
            @event.TicketTypes.Any() ? @event.TicketTypes.Sum(t => t.AvailableQuantity) : @event.Capacity,
            @event.AccessType,
            @event.Status,
            @event.Venue,
            @event.Image != null ? fileStorageService.GetFileUrl(@event.Image.FilePath) : null,
            @event.Badge,
            @event.PricingPolicyIds.ToList(),
            @event.TicketTypes.Select(t => new TicketTypeDto(
                t.Id,
                t.Category,
                t.BasePrice,
                t.TotalQuantity,
                t.AvailableQuantity,
                t.MaxPerMember,
                t.RequiresMembership,
                t.MinAge,
                t.MaxAge,
                t.GenderRestriction)).ToList()
        );

        return dto;
    }
}
