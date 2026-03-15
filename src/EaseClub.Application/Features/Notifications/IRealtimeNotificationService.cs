using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Notifications
{
    public interface IRealtimeNotificationService
    {
        Task SendToUserAsync(Guid userId, string title, string message);

        Task SendToClubAsync(Guid clubId, string title, string message);
    }
}
