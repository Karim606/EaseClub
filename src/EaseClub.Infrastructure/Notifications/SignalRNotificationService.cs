using EaseClub.Application.Features.Notifications;
using EaseClub.Infrastructure.Notifications.RealTime;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Notifications
{
    public class SignalRNotificationService : IRealtimeNotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public SignalRNotificationService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendToUserAsync(Guid userId, string title, string message)
        {
            await _hubContext.Clients
                .User(userId.ToString())
                .SendAsync("ReceiveNotification", new
                {
                    Title = title,
                    Message = message
                });
        }

        public async Task SendToClubAsync(Guid clubId, string title, string message)
        {
            await _hubContext.Clients
                .Group($"club-{clubId}")
                .SendAsync("ReceiveNotification", new
                {
                    Title = title,
                    Message = message
                });
        }
    }
}
