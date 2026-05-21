using EaseClub.Domain.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Notifications
{
    public interface INotificationDispatcher
    {
        public Task DispatchAsync(Notification notification);
    }
}
