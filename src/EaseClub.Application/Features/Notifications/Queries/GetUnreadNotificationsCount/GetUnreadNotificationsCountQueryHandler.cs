using Microsoft.Extensions.Logging;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.ClubAdmin;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Notifications.Queries.GetUnreadNotificationsCount
{
    public class GetUnreadNotificationsCountQueryHandler(INotificationQueryService queryService,
        ICurrentUserService currentUserService,
        IClubAdminUserRepository clubAdminUserRepository,
        ILogger<GetUnreadNotificationsCountQueryHandler> logger) : IRequestHandler<GetUnreadNotificationsCountQuery, Result<int>>
    {
        public async Task<Result<int>> Handle(GetUnreadNotificationsCountQuery request, CancellationToken cancellationToken)
        {
            if ((request.UserId == null && request.ClubId == null) || (request.UserId != null && request.ClubId != null))
                return Error.Validation(description: "UserId and ClubId cannot be null or have values at the same time give a value to one of them");

            var res = Guid.TryParse(currentUserService.GetId(), out var userId);
            if (!res)
            {
                logger.LogError("Unauthorized access in GetUnreadNotificationsCountQueryHandler: {Error}", Error.Unauthorized().ToLogObject());
                return Error.Unauthorized();
            }
            var roles = currentUserService.GetRoles();

            var isSuperAdmin = roles.Any(x => x == "SuperAdmin");

            if (isSuperAdmin)
            {
                return await queryService.GetUnreadNotificationsCountAsync(
                    request.UserId,
                    request.ClubId,
                    cancellationToken);
            }

            // User-based access
            if (request.UserId != null)
            {
                if (request.UserId != userId)
                {
                    logger.LogError("Forbidden error in GetUnreadNotificationsCountQueryHandler: {Error}", Error.Forbidden().ToLogObject());
                    return Error.Forbidden();
                }
            }

            // Club-based access
            if (request.ClubId != null)
            {
                if (!roles.Contains("ClubAdmin"))
                    return Error.Forbidden();

                var admin = await clubAdminUserRepository.GetByIdAsync(userId);

                if (admin == null || admin.ClubId != request.ClubId)
                {
                    logger.LogError("Forbidden error in GetUnreadNotificationsCountQueryHandler: {Error}", Error.Forbidden().ToLogObject());
                    return Error.Forbidden();
                }
            }

            // Execute query
            return await queryService.GetUnreadNotificationsCountAsync(
                request.UserId,
                request.ClubId,
                cancellationToken);
        }
    }
}
