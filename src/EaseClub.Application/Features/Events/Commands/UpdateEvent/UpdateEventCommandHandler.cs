using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Events.Commands.UpdateEvent;

public class UpdateEventCommandHandler(
    IEventRepository eventRepository,
    IUnitOfWork unitOfWork,
    ILogger<UpdateEventCommandHandler> logger)
    : IRequestHandler<UpdateEventCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdAsync(request.Id, cancellationToken);
        if (@event == null)
        {
            return Error.NotFound("Event.NotFound", "Event not found.");
        }

        var result = @event.UpdateDetails(
            request.Name,
            request.Description,
            request.StartDate,
            request.EndDate,
            request.Capacity);

        if (result.IsError)
        {
            logger.LogWarning("Failed to update event {EventId}: {Errors}", 
                request.Id, string.Join(", ", result.Errors.Select(e => e.Description)));
            return result.Errors.First();
        }

        await eventRepository.UpdateAsync(@event, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}
