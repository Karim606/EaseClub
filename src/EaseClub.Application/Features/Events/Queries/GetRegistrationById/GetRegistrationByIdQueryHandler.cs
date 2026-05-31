using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Features.Events.Dtos;
using EaseClub.Domain.Clubs;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Events;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Events.Queries.GetRegistrationById;

public class GetRegistrationByIdQueryHandler(IEventRepository eventRepository, IClubRepository clubRepository, IFileStorageService fileStorageService)
    : IRequestHandler<GetRegistrationByIdQuery, Result<EventRegistrationDto>>
{
    public async Task<Result<EventRegistrationDto>> Handle(GetRegistrationByIdQuery request, CancellationToken cancellationToken)
    {
        var r = await eventRepository.GetRegistrationByIdAsync(request.RegistrationId, cancellationToken);
        if (r == null) return Error.NotFound("Registration.NotFound", "Registration not found.");

        var club = await clubRepository.GetByIdAsync(r.Event.ClubId, cancellationToken);
        var clubName = club?.Name ?? string.Empty;

        var dto = new EventRegistrationDto(
            r.Id,
            r.EventId,
            r.Event.ClubId,
            r.RegistrantId,
            r.IsRegistrantAttending,
            r.Status,
            r.InvoiceId,
            r.TotalBasePrice,
            r.DiscountAmount,
            r.FinalTotal,
            r.AppliedPolicies,
            r.ReadableId,
            r.Event.Name,
            r.Event.StartDate,
            r.Event.Venue,
            r.Event.Image != null ? fileStorageService.GetFileUrl(r.Event.Image.FilePath) : null,
            clubName,
            r.Attendees.Select(a => new AttendeeDto(
                a.Id,
                a.TicketTypeId,
                r.Event.TicketTypes.First(t => t.Id == a.TicketTypeId).Category,
                a.AttendeeId,
                a.AttendeeName,
                a.Age,
                a.Gender
            )).ToList()
        );

        return dto;
    }
}
