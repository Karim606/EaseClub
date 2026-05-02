using EaseClub.Application.Features.Events.Dtos;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Events;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Events.Queries.GetRegistrationById;

public class GetRegistrationByIdQueryHandler(IEventRepository eventRepository)
    : IRequestHandler<GetRegistrationByIdQuery, Result<EventRegistrationDto>>
{
    public async Task<Result<EventRegistrationDto>> Handle(GetRegistrationByIdQuery request, CancellationToken cancellationToken)
    {
        var r = await eventRepository.GetRegistrationByIdAsync(request.RegistrationId, cancellationToken);
        if (r == null) return Error.NotFound("Registration.NotFound", "Registration not found.");

        var dto = new EventRegistrationDto(
            r.Id,
            r.EventId,
            r.RegistrantId,
            r.Status,
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
        );

        return dto;
    }
}
