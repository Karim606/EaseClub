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
            var roles = currentUserService.GetRoles();
            var res = Guid.TryParse(currentUserService.GetId(), out var userId);

            if(!res) return Error.Unauthorized();

            if (notification == null)
                return Error.NotFound();

            if (roles.Contains("ClubAmin"))
            {
                var admin = await clubAdminUserRepository.GetByIdAsync(userId);
                if (admin == null)
                    return Error.Unauthorized();

                if (admin.ClubId != notification.ClubId)
                    return Error.Forbidden();
            }
            else if (roles.Contains("Member")) { 
                                        
                  if(notification.UserId != userId)
                    return Error.Forbidden();
            }

                notification.MarkAsRead(); 
            await unitOfWork.SaveChangesAsync(ct);

            return Result.Success;
        }
    }
}
