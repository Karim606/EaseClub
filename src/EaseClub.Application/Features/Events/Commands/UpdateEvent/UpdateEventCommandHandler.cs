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
using EaseClub.Domain.Files;
using EaseClub.Domain.Files.Enums;

namespace EaseClub.Application.Features.Events.Commands.UpdateEvent;

public class UpdateEventCommandHandler(
    IEventRepository eventRepository,
    IFileRepository fileRepository,
    IUnitOfWork unitOfWork,
    ILogger<UpdateEventCommandHandler> logger)
    : IRequestHandler<UpdateEventCommand, Result<EventActionResponseDto>>
{
    public async Task<Result<EventActionResponseDto>> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdAsync(request.Id, cancellationToken);
        if (@event == null)
        {
            return Error.NotFound("Event.NotFound", "Event not found.");
        }

        var oldImageId = @event.ImageId;

        var result = @event.UpdateDetails(
            request.Name,
            request.Description,
            request.StartDate,
            request.EndDate,
            request.Capacity,
            request.Venue,
            request.ImageId,
            request.Badge);

        if (result.IsError)
        {
            logger.LogWarning("Failed to update event {EventId}: {Errors}", 
                request.Id, string.Join(", ", result.Errors.Select(e => e.Description)));
            return result.Errors.First();
        }


        if (request.ImageId.HasValue)
        {
            var imageFile = await fileRepository.GetByIdAsync(request.ImageId.Value, cancellationToken);
            if (imageFile == null)
            {
                return Error.NotFound("Image file not found.");
            }
            if (imageFile.OwnerType != FileOwnerType.Club || imageFile.OwnerId != @event.ClubId)
            {
                return Error.Unauthorized("File does not belong to this club");
            }
            imageFile.MarkAsPermanent();
        }

        if (oldImageId.HasValue && oldImageId.Value != request.ImageId)
        {
            var oldImage = await fileRepository.GetByIdAsync(oldImageId.Value, cancellationToken);
            if (oldImage != null)
            {
                await fileRepository.DeleteAsync(oldImage, cancellationToken);
            }
        }

        await eventRepository.UpdateAsync(@event, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new EventActionResponseDto(@event.Id, @event.Name);
    }
}
