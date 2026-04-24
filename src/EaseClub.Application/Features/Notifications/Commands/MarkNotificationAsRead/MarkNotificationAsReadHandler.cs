using EaseClub.Application.Common.Interfaces;
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

namespace EaseClub.Application.Features.Notifications.Commands.MarkNotificationAsRead
{
    public class MarkNotificationAsReadCommandHandler(INotificationRepository notificationRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IClubAdminUserRepository clubAdminUserRepository) : IRequestHandler<MarkNotificationAsReadCommand,Result<Success>>
    {
        public async Task<Result<Success>> Handle(MarkNotificationAsReadCommand request, CancellationToken ct)
        {
            var notification = await notificationRepository.GetByIdAsync(request.NotificationId);

            if (notification == null)
                return Error.NotFound();

            var roles = currentUserService.GetRoles();

            if (!Guid.TryParse(currentUserService.GetId(), out var userId))
                return Error.Unauthorized();

            var isClubAdmin = roles.Contains("ClubAdmin");
            var isMember = roles.Contains("Member");

            // CLUB ADMIN ACCESS
            if (isClubAdmin)
            {
                var admin = await clubAdminUserRepository.GetByIdAsync(userId);

                if (admin == null)
                    return Error.Unauthorized();

                if (!notification.ClubId.HasValue || admin.ClubId != notification.ClubId)
                    return Error.Forbidden();
            }

            // MEMBER ACCESS
            else if (isMember)
            {
                if (!notification.UserId.HasValue || notification.UserId != userId)
                    return Error.Forbidden();
            }

            // Optional: deny unknown roles
            else
            {
                return Error.Forbidden();
            }

            notification.MarkAsRead();

            await unitOfWork.SaveChangesAsync(ct);

            return Result.Success;
        }

    }
}
