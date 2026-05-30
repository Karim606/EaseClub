using EaseClub.Application.Common.Pagination;
using EaseClub.Application.Features.Notifications.Queries.GetNotifications;
using EaseClub.Domain.Common.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Notifications.Queries
{
    public interface INotificationQueryService
    {
        Task<Result<UnifiedPaginatedResponse<NotificationDto>>> GetNotificationsAsync(Guid? userId, Guid? clubId, bool? isRead, PaginationRequest parameters, CancellationToken ct = default);
        Task<Result<int>> GetUnreadNotificationsCountAsync(Guid? userId, Guid? clubId, CancellationToken ct = default);
    }
}
