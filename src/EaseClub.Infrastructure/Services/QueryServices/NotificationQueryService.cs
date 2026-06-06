using EaseClub.Application.Common.Pagination;
using EaseClub.Application.Features.Notifications.Queries;
using EaseClub.Application.Features.Notifications.Queries.GetNotifications;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Notifications;
using EaseClub.Infrastructure.Common.QueryServices;
using EaseClub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Services.QueryServices
{
    public class NotificationQueryService :BaseQueryService<Notification>, INotificationQueryService
    {
        public NotificationQueryService(AppDbContext context,ILogger<NotificationQueryService> logger) : base(context,logger) { }
        public async Task<Result<UnifiedPaginatedResponse<NotificationDto>>> GetNotificationsAsync(Guid? userId,Guid? clubId,bool? isRead, PaginationRequest parameters,CancellationToken ct = default)
        {
            var query = Query();

            if (userId != null)
            {
                query = query.Where(n => n.UserId == userId);
            }
            else if (clubId != null) { 
                query = query.Where(n => n.ClubId == clubId);
            }

            if (isRead != null)
            {
                query = query.Where(n => n.IsRead == isRead);
            }

                var unifiedResponse = await GetUnifiedPaginatedAsync<NotificationDto, DateTime>(query, parameters, selector: n => new NotificationDto(n.Id, n.ClubId, n.UserId, n.IsRead, n.Title, n.Message, n.CreatedAt, n.Type, n.MetadataJson), orderSelector: n => n.CreatedAt,ct);
                
            if(unifiedResponse.IsError) return unifiedResponse.TopError;
            return unifiedResponse;
        }

        public async Task<Result<int>> GetUnreadNotificationsCountAsync(Guid? userId, Guid? clubId, CancellationToken ct = default)
        {
            var query = Query().Where(n => !n.IsRead);

            if (userId != null)
            {
                query = query.Where(n => n.UserId == userId);
            }
            else if (clubId != null)
            {
                query = query.Where(n => n.ClubId == clubId);
            }

            try
            {
                var count = await query.CountAsync(ct);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread notifications count");
                return Error.Unexpected("Error getting unread notifications count");
            }
        }
    }
}
