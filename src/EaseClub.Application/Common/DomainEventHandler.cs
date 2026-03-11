using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Common
{
    public abstract class DomainEventHandler<TEvent,THandler> : INotificationHandler<TEvent>
    where TEvent : DomainEvent
    {
        //protected readonly INotificationService _notificationService;
        protected readonly ILogger<THandler> _logger;

        protected DomainEventHandler(ILogger<THandler> logger)
        {
            _logger = logger;
        }

        public async Task Handle(TEvent evt, CancellationToken ct)
        {
            LogEvent(evt);

            try
            {
                await HandleEvent(evt, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing event {EventId}", evt.EventId);
                throw;
            }
        }

        protected abstract Task HandleEvent(TEvent evt, CancellationToken ct);


        protected void LogEvent(TEvent evt)
        {
             _logger.LogInformation(GetType().Name + " - " + $", event with Id:{evt.EventId} of type:{evt.GetType().Name} that ocuured on:" +
                 $"{evt.OccurredOn}" +
                 "is currently processed");
        }

        //protected async Task NotifyUserAsync(Guid userId, string message)
        //{
        //    await _notificationService.SendAsync(userId, message);
        //}
    }
}
