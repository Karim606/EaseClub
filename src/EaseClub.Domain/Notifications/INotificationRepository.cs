using EaseClub.Domain.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Notifications
{
    public interface INotificationRepository:IRepository<Notification>
    {
        public Task<List<Notification>> GetUnReadNotificationsAsync(Guid? userId,Guid? clubId, CancellationToken ct = default);
    }
}
