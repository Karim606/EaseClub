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
            var res = Guid.TryParse(currentUserService.GetId(), out var userId);

            if (!res) return Error.Unauthorized();

            if ((request.UserId == null && request.ClubId == null) || (request.UserId != null && request.ClubId != null)) return Error.Validation(description: "UserId and ClubId cannot be null or have values at the same time give a value to one of them");

            var roles = currentUserService.GetRoles();
            List<Notification> notifications = new List<Notification>();
            var isSuperAdmin = roles.Contains("SuperAdmin");

            if (isSuperAdmin)
            {
                notifications = await notificationRepository.GetUnReadNotificationsAsync(request.UserId, request.ClubId, ct);
            }
            else if (roles.Contains("ClubAdmin"))
            {
                var admin = await clubAdminUserRepository.GetByIdAsync(userId);
                if (admin == null) return Error.Unauthorized();
                if (admin.ClubId != request.ClubId) return Error.Forbidden();
                notifications = await notificationRepository.GetUnReadNotificationsAsync(request.UserId, request.ClubId, ct);
            }
            else if (roles.Contains("Member"))
            {
                if (request.UserId != userId) return Error.Forbidden();
                notifications = await notificationRepository.GetUnReadNotificationsAsync(userId, request.ClubId, ct);
            }
            else
            {
                return Error.Forbidden();
            }

            foreach (var notification in notifications)
                    notification.MarkAsRead();

            await unitOfWork.SaveChangesAsync(ct);

            return Result.Success;
        }
    }
}
