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

namespace EaseClub.Application.Features.Events.Commands.CreateEvent;

public class CreateEventCommandHandler(
    IEventRepository eventRepository,
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
            request.Audience,
            request.Venue,
            request.ImageUrl,
            request.Badge,
            request.IsFeatured);

        if (eventResult.IsError)
        {
            logger.LogWarning("Failed to create event: {Errors}", 
                string.Join(", ", eventResult.Errors.Select(e => e.Description)));
            return eventResult.Errors.First();
        }
        
        await eventRepository.AddAsync(eventResult.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new EventActionResponseDto(eventResult.Value.Id, eventResult.Value.Name);
    }
}
