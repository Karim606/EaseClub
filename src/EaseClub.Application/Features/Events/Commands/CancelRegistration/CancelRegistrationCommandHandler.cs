using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Events.Commands.CancelRegistration;

public class CancelRegistrationCommandHandler(
    IEventRepository eventRepository,
    IUnitOfWork unitOfWork,
    ILogger<CancelRegistrationCommandHandler> logger)
    : IRequestHandler<CancelRegistrationCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(CancelRegistrationCommand request, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetWithDetailsAsync(request.EventId, cancellationToken);
        if (@event == null)
        {
            return Error.NotFound("Event.NotFound", "Event not found.");
        }

        var result = @event.CancelRegistration(request.RegistrationId);

        if (result.IsError)
        {
            logger.LogWarning("Failed to cancel registration {RegistrationId} for event {EventId}: {Errors}", 
                request.RegistrationId, request.EventId, string.Join(", ", result.Errors.Select(e => e.Description)));
            return result.Errors.First();
        }

        await eventRepository.UpdateAsync(@event, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}
