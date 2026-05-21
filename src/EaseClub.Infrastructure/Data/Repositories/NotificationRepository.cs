using EaseClub.Domain.Notifications;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Data.Repositories
{
    public class NotificationRepository : EfRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<Notification>> GetUnReadNotificationsAsync(Guid? userId,Guid? clubId,CancellationToken ct = default)
        {
            IQueryable<Notification> query = _context.Notifications
                .Where(n => !n.IsRead);

            if (userId.HasValue)
            {
                query = query.Where(n => n.UserId == userId);
            }

            if (clubId.HasValue)
            {
                query = query.Where(n => n.ClubId == clubId);
            }

            return await query.ToListAsync(ct);


        }
    }
}
