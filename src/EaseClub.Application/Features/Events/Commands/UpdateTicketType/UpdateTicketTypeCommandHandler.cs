using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Events.Commands.UpdateTicketType;

public class UpdateTicketTypeCommandHandler(
    IEventRepository eventRepository,
    IUnitOfWork unitOfWork,
    ILogger<UpdateTicketTypeCommandHandler> logger)
    : IRequestHandler<UpdateTicketTypeCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(UpdateTicketTypeCommand request, CancellationToken cancellationToken)
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
            request.Price,
            request.Quantity,
            request.MaxPerMember);

        if (result.IsError)
        {
            logger.LogWarning("Failed to update ticket type {TicketTypeId} for event {EventId}: {Errors}", 
                request.TicketTypeId, request.EventId, string.Join(", ", result.Errors.Select(e => e.Description)));
            return result.Errors.First();
        }

        await eventRepository.UpdateAsync(@event, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}
