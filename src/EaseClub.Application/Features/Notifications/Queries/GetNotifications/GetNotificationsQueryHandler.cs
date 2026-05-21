using Microsoft.Extensions.Logging;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Common.Pagination;
using EaseClub.Domain.ClubAdmin;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Notifications.Queries.GetNotifications
{
    public class GetNotificationsQueryHandler(INotificationQueryService queryService,
        ICurrentUserService currentUserService,
        IClubAdminUserRepository clubAdminUserRepository,
        ILogger<GetNotificationsQueryHandler> logger) : IRequestHandler<GetNotificationsQuery, Result< UnifiedPaginatedResponse<NotificationDto> >>
    {
        public async Task<Result<UnifiedPaginatedResponse<NotificationDto>>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
        {
            if((request.UserId==null&&request.ClubId==null)||(request.UserId!=null&&request.ClubId!=null)) return Error.Validation(description:"UserId and ClubId cannot be null or have values at the same time give a value to one of them");

            var res = Guid.TryParse(currentUserService.GetId(),out var userId);
            if (!res) { logger.LogError("Unauthorized access in GetNotificationsQueryHandler: {Error}", Error.Unauthorized().ToLogObject()); return Error.Unauthorized(); }
            var roles = currentUserService.GetRoles();

            var isSuperAdmin = roles.Any(x => x == "SuperAdmin");

            if (isSuperAdmin)
            {
                return await queryService.GetNotificationsAsync(
                    request.UserId,
                    request.ClubId,
                    request.IsRead,
                    request.PaginationParameters);
            }

            // 4. User-based access
            if (request.UserId != null)
            {
                if (request.UserId != userId) { logger.LogError("Forbidden error in GetNotificationsQueryHandler: {Error}", Error.Forbidden().ToLogObject()); return Error.Forbidden(); }
            }

            // 5. Club-based access
            if (request.ClubId != null)
            {
                if (!roles.Contains("ClubAdmin"))
                    return Error.Forbidden();

                var admin = await clubAdminUserRepository.GetByIdAsync(userId);

                if (admin == null || admin.ClubId != request.ClubId) { logger.LogError("Forbidden error in GetNotificationsQueryHandler: {Error}", Error.Forbidden().ToLogObject()); return Error.Forbidden(); }
            }

            // 6. Execute query
            return await queryService.GetNotificationsAsync(
                request.UserId,
                request.ClubId,
                request.IsRead,
                request.PaginationParameters);
        }
    }
}
