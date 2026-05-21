using EaseClub.Application.Features.Notifications;
using EaseClub.Domain.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Notifications
{
    public class NotificationDispatcher : INotificationDispatcher
    {
        private readonly IRealtimeNotificationService _realtime;
        private readonly IPushNotificationService _push;

        public NotificationDispatcher(
            IRealtimeNotificationService realtime,
            IPushNotificationService push)
        {
            _realtime = realtime;
            _push = push;
        }

        public async Task DispatchAsync(Notification notification)
        {
            if (notification.UserId != null)
            {
                await _push.SendToUserAsync(notification.UserId.Value, notification.Title,notification.Message);
            }

            if (notification.ClubId != null)
            {
                await _realtime.SendToClubAsync(notification.ClubId.Value, notification.Title,notification.Message);
            }
        }
    }
}
