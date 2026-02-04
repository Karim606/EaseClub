using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.ClubAdmin;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipTypes;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipTypes.Commands.DeactivateMembershipType
{
    public class DeactivateMembershipTypeCommandHandler(
        IMembershipTypeRepository membershipTypeRepository,
        ILogger<DeactivateMembershipTypeCommandHandler>logger,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IClubAdminUserRepository clubAdminUserRepository
        ) :IRequestHandler<DeactivateMembershipTypeCommand, Result<Success>>
    {
        public async Task<Result<Success>> Handle(DeactivateMembershipTypeCommand request, CancellationToken cancellationToken)
        {
           var id = currentUserService.GetId();
            
            var parseResult = Guid.TryParse(id,out Guid clubAdminUserId);
            if(!parseResult)
            {
                logger.LogWarning("Invalid user ID format: {UserId}", id);
                return Error.Unauthorized(description: "User is not authenticated.");
            }

            var clubAdminUser = await clubAdminUserRepository.GetByIdAsync(clubAdminUserId);
            if(clubAdminUser is null)
            {
                logger.LogWarning("Club admin user not found for ID: {UserId}", clubAdminUserId);
                return Error.Unauthorized(description: "User is not authorized.");
            }

            var type = await membershipTypeRepository.GetByIdAsync(request.MembershipTypeId);

            if(type == null) { 
                logger.LogWarning("Membership type with ID {MembershipTypeId} not found.", request.MembershipTypeId);
                return Error.NotFound(description: "Membership type not found.");
            }

            if(type.ClubId != clubAdminUser.ClubId)
            {
                logger.LogWarning("User {UserId} attempted to deactivate membership type {MembershipTypeId} which does not belong to their club.", clubAdminUserId, request.MembershipTypeId);
                return Error.Forbidden(description: "You do not have permission to deactivate this membership type.");
            }

            type.Deactivate();
            await unitOfWork.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Membership type with ID {MembershipTypeId} has been deactivated by user {UserId}.", request.MembershipTypeId, clubAdminUserId);
            
            return Result.Success;
        }
    }
}
