using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Events.Commands.RemoveTicketType;

public class RemoveTicketTypeCommandHandler(
    IEventRepository eventRepository,
    IUnitOfWork unitOfWork,
    ILogger<RemoveTicketTypeCommandHandler> logger)
    : IRequestHandler<RemoveTicketTypeCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(RemoveTicketTypeCommand request, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetWithDetailsAsync(request.EventId, cancellationToken);
        if (@event == null)
        {
            return Error.NotFound("Event.NotFound", "Event not found.");
        }

        var result = @event.RemoveTicketType(request.TicketTypeId);

        if (result.IsError)
        {
            logger.LogWarning("Failed to remove ticket type {TicketTypeId} from event {EventId}: {Errors}", 
                request.TicketTypeId, request.EventId, string.Join(", ", result.Errors.Select(e => e.Description)));
            return result.Errors.First();
        }

        await eventRepository.UpdateAsync(@event, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}
