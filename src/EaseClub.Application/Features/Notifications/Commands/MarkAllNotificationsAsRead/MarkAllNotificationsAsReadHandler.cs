using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Features.Notifications.Commands.MarkNotificationAsRead;
using EaseClub.Domain.ClubAdmin;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Notifications;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Notifications.Commands.MarkAllNotificationsAsRead
{
    public class MarkNotificationAsReadCommandHandler(INotificationRepository notificationRepository,
       IUnitOfWork unitOfWork,
       ICurrentUserService currentUserService,
       IClubAdminUserRepository clubAdminUserRepository) : IRequestHandler<MarkAllNotificationsAsReadCommand, Result<Success>>
    {
        public async Task<Result<Success>> Handle(MarkAllNotificationsAsReadCommand request, CancellationToken ct)
        {
            // 1. Validate request shape
            if ((request.UserId == null && request.ClubId == null) ||
                (request.UserId != null && request.ClubId != null))
            {
                return Error.Validation(
                    description: "Provide either UserId or ClubId, not both.");
            }

            // 2. Validate current user
            if (!Guid.TryParse(currentUserService.GetId(), out var userId))
            {
                return Error.Unauthorized();
            }

            var roles = currentUserService.GetRoles();
            var isSuperAdmin = roles.Contains("SuperAdmin");

            // 3. Authorization rules (skip for SuperAdmin)
            if (!isSuperAdmin)
            {
                // User-level access
                if (request.UserId != null)
                {
                    if (request.UserId != userId)
                        return Error.Forbidden();
                }

                // Club-level access
                if (request.ClubId != null)
                {
                    if (!roles.Contains("ClubAdmin"))
                        return Error.Forbidden();

                    var admin = await clubAdminUserRepository.GetByIdAsync(userId);

                    if (admin == null || admin.ClubId != request.ClubId)
                        return Error.Forbidden();
                }
            }

            // 4. Resolve target scope safely
            var targetUserId = request.UserId ?? userId;

            // 5. Fetch unread notifications (single source of truth)
            var notifications = await notificationRepository.GetUnReadNotificationsAsync(
                targetUserId,
                request.ClubId,
                ct);

            // 6. Mark as read
            foreach (var notification in notifications)
            {
                notification.MarkAsRead();
            }

            // 7. Persist changes
            await unitOfWork.SaveChangesAsync(ct);

            return Result.Success;
        }
    }
}
