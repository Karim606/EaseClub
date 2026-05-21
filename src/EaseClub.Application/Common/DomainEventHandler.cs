using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Features.Notifications;
using EaseClub.Domain.Notifications;
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
    public abstract class DomainEventHandler<TEvent, THandler> : INotificationHandler<TEvent>
    where TEvent : DomainEvent
    {
        protected readonly INotificationDispatcher _notificationDispatcher;
        protected readonly INotificationRepository _notificationRepository;
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly ILogger<THandler> _logger;

        protected DomainEventHandler(INotificationDispatcher notificationDispatcher, INotificationRepository notificationRepository, IUnitOfWork unitOfWork, ILogger<THandler> logger)
        {
            _notificationDispatcher = notificationDispatcher;
            _notificationRepository = notificationRepository;
            _unitOfWork = unitOfWork;
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

        protected async Task DispatchNotification(Notification notification, CancellationToken ct)
        {
            await _notificationRepository.AddAsync(notification, ct);
            await SaveChangesAsync(ct);
            await _notificationDispatcher.DispatchAsync(notification);
        }

        protected async Task SaveChangesAsync(CancellationToken ct)
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}
