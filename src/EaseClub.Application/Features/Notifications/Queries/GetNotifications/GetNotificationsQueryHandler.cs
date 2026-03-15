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
        IClubAdminUserRepository clubAdminUserRepository) : IRequestHandler<GetNotificationsQuery, Result< UnifiedPaginatedResponse<NotificationDto> >>
    {
        public async Task<Result<UnifiedPaginatedResponse<NotificationDto>>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
        {
            if((request.UserId==null&&request.ClubId==null)||(request.UserId!=null&&request.ClubId!=null)) return Error.Validation(description:"UserId and ClubId cannot be null or have values at the same time give a value to one of them");

            var res = Guid.TryParse(currentUserService.GetId(),out var UserId);
            if (!res) return Error.Unauthorized();

            var roles = currentUserService.GetRoles();

            var isSuperAdmin = roles.Any(x => x == "SuperAdmin");

            if(!isSuperAdmin && request.UserId!=null&&UserId != request.UserId) return Error.Forbidden();
            else if (request.ClubId != null && roles.Contains("ClubAdmin"))
            {
                var admin = await clubAdminUserRepository.GetByIdAsync(UserId);
                if (admin.ClubId != request.ClubId) return Error.Forbidden();
            }
            else if(!isSuperAdmin) return Error.Forbidden();


                return await queryService.GetNotificationsAsync(request.UserId, request.ClubId, request.IsRead, request.PaginationParameters);
        }
    }
}
