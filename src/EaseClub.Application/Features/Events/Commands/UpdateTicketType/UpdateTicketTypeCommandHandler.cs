using EaseClub.Application.Features.Events.Dtos;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace EaseClub.Application.Features.Events.Commands.UpdateTicketType;

public class UpdateTicketTypeCommandHandler(
    IEventRepository eventRepository,
    IUnitOfWork unitOfWork,
    ILogger<UpdateTicketTypeCommandHandler> logger)
    : IRequestHandler<UpdateTicketTypeCommand, Result<EventActionResponseDto>>
{
    public async Task<Result<EventActionResponseDto>> Handle(UpdateTicketTypeCommand request, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetWithDetailsAsync(request.EventId, cancellationToken);
        if (@event == null)
        {
            return Error.NotFound("Event.NotFound", "Event not found.");
        }

        var result = @event.UpdateTicketType(
            request.TicketTypeId,
            request.Name,
            request.Description,
            request.BasePrice,
            request.TotalQuantity,
            request.MaxPerMember,
            request.RequiresMembership,
            request.MinAge,
            request.MaxAge,
            request.GenderRestriction);

        if (result.IsError)
        {
            logger.LogWarning("Failed to update ticket type {TicketTypeId} for event {EventId}: {Errors}", 
                request.TicketTypeId, request.EventId, string.Join(", ", result.Errors.Select(e => e.Description)));
            return result.Errors.First();
        }

        await eventRepository.UpdateAsync(@event, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new EventActionResponseDto(request.TicketTypeId, request.Name);
    }
}
