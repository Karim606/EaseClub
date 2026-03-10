using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Common
{
    public abstract class DomainEventHandler<TEvent> : INotificationHandler<TEvent>
    where TEvent : DomainEvent
    {
        //protected readonly INotificationService _notificationService;
        //protected readonly IAuditLogger _auditLogger;

        //public DomainEventHandler(INotificationService notificationService,
        //                          IAuditLogger auditLogger)
        //{
        //    _notificationService = notificationService;
        //    _auditLogger = auditLogger;
        //}

        public abstract Task Handle(TEvent evt, CancellationToken ct);

        //protected async Task LogEventAsync(TEvent evt)
        //{
        //    await _auditLogger.LogAsync(evt.EventId, evt.GetType().Name, evt.OccurredOn);
        //}

        //protected async Task NotifyUserAsync(Guid userId, string message)
        //{
        //    await _notificationService.SendAsync(userId, message);
        //}
    }
}
