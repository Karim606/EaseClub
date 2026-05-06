using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Clubs;
using EaseClub.Domain.Files;
using EaseClub.Domain.Files.Enums;
using EaseClub.Domain.Member;
using EaseClub.Domain.MembershipApplications.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Files.Commands
{
    public class FileAuthorizationService(IMemberUserRepository userRepo,IClubAuthorizationService authorizationService,
        IClubRepository clubRepo,IMembershipApplicationRepository appRepo,ICurrentUserService currentUserService)
    {
        public async Task<bool> CanAccessAsync(FileResource fileResource)
        {
            if (!fileResource.IsPrivate) return true;

            var parsingRes = Guid.TryParse(currentUserService.GetId(), out var userId);
            if (!parsingRes) return false;

            var roles =  currentUserService.GetRoles();
            if(roles.Contains("SuperAdmin"))return true;

            switch (fileResource.OwnerType)
            {
                case FileOwnerType.Club:
                    var club = await clubRepo.GetByIdAsync(fileResource.OwnerId);
                    if (club == null) return false;
                    var isAdmin = await authorizationService.IsUserAdminOfClubAsync(userId, club.Id);
                    return isAdmin;
                case FileOwnerType.Application:
                    var app = await appRepo.GetByIdAsync(fileResource.OwnerId);
                    if (app == null) return false;
                    return app.MemberId == userId || await authorizationService.IsUserAdminOfClubAsync(userId, app.ClubId);

                case FileOwnerType.User:
                    return fileResource.OwnerId == userId;
                default:
                    return false;
            }
        }
    }

}
