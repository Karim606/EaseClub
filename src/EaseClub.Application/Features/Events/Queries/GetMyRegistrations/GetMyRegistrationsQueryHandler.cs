using EaseClub.Application.Features.Events.Dtos;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Events;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Events.Queries.GetMyRegistrations;

public class GetMyRegistrationsQueryHandler(IEventRepository eventRepository)
    : IRequestHandler<GetMyRegistrationsQuery, Result<List<EventRegistrationDto>>>
{
    public async Task<Result<List<EventRegistrationDto>>> Handle(GetMyRegistrationsQuery request, CancellationToken cancellationToken)
    {
        var registrations = await eventRepository.GetRegistrationsByUserIdAsync(request.UserId, cancellationToken);
        
        var dtos = registrations.Select(r => new EventRegistrationDto(
            r.Id,
            r.EventId,
            r.RegistrantId,
            r.Status,
            r.TotalBasePrice,
            r.Attendees.Select(a => new AttendeeDto(
                a.Id,
                a.TicketTypeId,
                a.AttendeeId,
                a.AttendeeName
            )).ToList()
        )).ToList();

        return dtos;
    }
}
