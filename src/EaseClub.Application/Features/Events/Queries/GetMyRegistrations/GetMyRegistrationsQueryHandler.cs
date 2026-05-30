using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Features.Events.Dtos;
using EaseClub.Domain.Clubs;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Events;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Events.Queries.GetMyRegistrations;

public class GetMyRegistrationsQueryHandler(IEventRepository eventRepository, IClubRepository clubRepository, IFileStorageService fileStorageService)
    : IRequestHandler<GetMyRegistrationsQuery, Result<List<EventRegistrationDto>>>
{
    public async Task<Result<List<EventRegistrationDto>>> Handle(GetMyRegistrationsQuery request, CancellationToken cancellationToken)
    {
        var registrations = await eventRepository.GetRegistrationsByUserIdAsync(request.UserId, cancellationToken);
        
        var clubIds = registrations.Select(r => r.Event.ClubId).Distinct().ToList();
        var clubNames = new Dictionary<Guid, string>();
        foreach (var clubId in clubIds)
        {
            var club = await clubRepository.GetByIdAsync(clubId, cancellationToken);
            clubNames[clubId] = club?.Name ?? string.Empty;
        }

        var dtos = registrations.Select(r => new EventRegistrationDto(
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
            clubNames.GetValueOrDefault(r.Event.ClubId, string.Empty),
            r.Attendees.Select(a => new AttendeeDto(
                a.Id,
                a.TicketTypeId,
                a.AttendeeId,
                a.AttendeeName,
                a.Age,
                a.Gender
            )).ToList()
        )).ToList();

        return dtos;
    }
}
