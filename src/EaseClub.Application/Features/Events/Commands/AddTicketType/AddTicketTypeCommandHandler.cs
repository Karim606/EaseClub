using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Events.Commands.AddTicketType;

public class AddTicketTypeCommandHandler(
    IEventRepository eventRepository,
    IUnitOfWork unitOfWork,
    ILogger<AddTicketTypeCommandHandler> logger)
    : IRequestHandler<AddTicketTypeCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(AddTicketTypeCommand request, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetWithDetailsAsync(request.EventId, cancellationToken);
        if (@event == null)
        {
            return Error.NotFound("Event.NotFound", "Event not found.");
        }

        var result = @event.AddTicketType(
            request.Name,
            request.Description,
            request.Category,
            request.Price,
            request.Quantity,
            request.MaxPerMember);

        if (result.IsError)
        {
            logger.LogWarning("Failed to add ticket type to event {EventId}: {Errors}", 
                request.EventId, string.Join(", ", result.Errors.Select(e => e.Description)));
            return result.Errors.First();
        }

        await eventRepository.UpdateAsync(@event, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return result.Value.Id;
    }
}
