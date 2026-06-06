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

namespace EaseClub.Application.Features.Events.Commands.CancelEvent;

public class CancelEventCommandHandler(
    IEventRepository eventRepository,
    IUnitOfWork unitOfWork,
    ILogger<CancelEventCommandHandler> logger)
    : IRequestHandler<CancelEventCommand, Result<EventActionResponseDto>>
{
    public async Task<Result<EventActionResponseDto>> Handle(CancelEventCommand request, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetWithDetailsAsync(request.Id, cancellationToken);
        if (@event == null)
        {
            return Error.NotFound("Event.NotFound", "Event not found.");
        }

        var result = @event.Cancel();

        if (result.IsError)
        {
            logger.LogWarning("Failed to cancel event {EventId}: {Errors}", 
                request.Id, string.Join(", ", result.Errors.Select(e => e.Description)));
            return result.Errors.First();
        }

        await eventRepository.UpdateAsync(@event, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new EventActionResponseDto(@event.Id, @event.Name);
    }
}
