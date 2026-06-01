using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EaseClub.Application.Features.Events.Dtos;
using EaseClub.Domain.Files;
using EaseClub.Domain.Files.Enums;
using EaseClub.Domain.Common;

namespace EaseClub.Application.Features.Events.Commands.CreateEvent;

public class CreateEventCommandHandler(
    IEventRepository eventRepository,
    IFileRepository fileRepository,
    IUnitOfWork unitOfWork,
    ILogger<CreateEventCommandHandler> logger)
    : IRequestHandler<CreateEventCommand, Result<EventActionResponseDto>>
{
    public async Task<Result<EventActionResponseDto>> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        var eventResult = Event.Create(
            request.ClubId,
            request.Name,
            request.Description,
            request.StartDate,
            request.EndDate,
            request.Capacity,
            request.Venue,
            request.ImageId,
            request.Badge);

        if (eventResult.IsError)
        {
            logger.LogWarning("Failed to create event: {Errors}", 
                string.Join(", ", eventResult.Errors.Select(e => e.Description)));
            return eventResult.Errors.First();
        }

        var @event = eventResult.Value;

        if (request.ImageId.HasValue)
        {
            var imageFile = await fileRepository.GetByIdAsync(request.ImageId.Value, cancellationToken);
            if (imageFile == null)
            {
                return Error.NotFound("Image file not found.");
            }
            if (imageFile.OwnerType != FileOwnerType.Club || imageFile.OwnerId != request.ClubId)
            {
                return Error.Unauthorized("File does not belong to this club");
            }
            imageFile.MarkAsPermanent();
        }
        
        await eventRepository.AddAsync(@event, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new EventActionResponseDto(@event.Id, @event.Name);
    }
}
