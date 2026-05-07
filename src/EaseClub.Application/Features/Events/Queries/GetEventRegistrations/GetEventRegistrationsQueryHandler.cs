using EaseClub.Application.Features.Events.Dtos;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Events;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Events.Queries.GetEventRegistrations;

public class GetEventRegistrationsQueryHandler(IEventRepository eventRepository)
    : IRequestHandler<GetEventRegistrationsQuery, Result<List<EventRegistrationDto>>>
{
    public async Task<Result<List<EventRegistrationDto>>> Handle(GetEventRegistrationsQuery request, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetWithDetailsAsync(request.EventId, cancellationToken);
        if (@event == null) return Error.NotFound("Event.NotFound", "Event not found.");

        var registrations = await eventRepository.GetRegistrationsByEventIdAsync(request.EventId, cancellationToken);
        
        var dtos = registrations.Select(r => new EventRegistrationDto(
            r.Id,
            r.EventId,
            r.RegistrantId,
            r.IsRegistrantAttending,
            r.Status,
            r.InvoiceId,
            r.TotalBasePrice,
            r.DiscountAmount,
            r.FinalTotal,
            r.AppliedPolicies,
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
