using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Branches;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipTypes;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipTypes.Commands.CreateMembershipType
{
    public class CreateMembershipTypeCommandHandler(IMembershipTypeRepository membershipTypeRepository,
        IBranchRepository branchRepository,
        IUnitOfWork unitOfWork,ILogger<CreateMembershipTypeCommandHandler>logger)
        :IRequestHandler<CreateMembershipTypeCommand,Result<Guid>>
    {
       
        public async Task<Result<Guid>> Handle(CreateMembershipTypeCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Creating membership type {MembershipTypeName} for club {ClubId}",request.Name,request.ClubId);

            var type = await membershipTypeRepository.GetByClubIdAndNameAsync(request.ClubId, request.Name);

            if (type != null)
            {
                logger.LogWarning("Membership type creation failed. A membership type with name {MembershipTypeName} " +
                    "already exists for club {ClubId}",request.Name,request.ClubId);

                return MembershipTypeErrors.MembershipTypeNameMustBeUniquePerClub;
            }

            var result = MembershipType.Create(Guid.NewGuid(), request.ClubId, request.Name);

            if (result.IsError) {
                logger.LogWarning("failed to create membership type: {reason}", result.TopError);
                return result.TopError;
                    }

            type = result.Value;
            type.UpdateDescription(request.Description);

            if (request.FamilyAllowed && request.MaxFamilyMembers.HasValue && request.MaxFamilyMembers.Value > 0)
            {
                logger.LogInformation("Enabling family membership for membership type {MembershipTypeName} " +
                    "with max family members {MaxFamilyMembers}",request.Name,request.MaxFamilyMembers.Value);
                var resultOfEnablingFamily =   type.EnableFamily(request.MaxFamilyMembers.Value);

                if (resultOfEnablingFamily.IsError)
                {
                    logger.LogWarning("Failed to enable family membership for membership type {MembershipTypeName}: {reason}",
                    request.Name,resultOfEnablingFamily.TopError);
                    return resultOfEnablingFamily.TopError;
                }
            }

            //  Branch restriction logic
            if (!request.AllBranchesPermitted)
            {
                var requestedBranchIds = request.BranchIds?
                    .Distinct()
                    .ToList() ?? new List<Guid>();

                if (requestedBranchIds.Count == 0)
                    return MembershipTypeErrors.RestrictedToBranchListMustBeGreaterThanZero;

                //  Validate branch existence
                var existingBranchIds = await branchRepository
                    .GetExistingBranchIdsAsync(requestedBranchIds);

                if (existingBranchIds.Count != requestedBranchIds.Count)
                {
                    var missingIds = requestedBranchIds.Except(existingBranchIds);

                    logger.LogWarning(
                        "Invalid branch ids provided when creating membership type {MembershipTypeName}: {BranchIds}",
                        request.Name,
                        string.Join(", ", missingIds));

                    return MembershipTypeErrors.BranchDoesNotExist(missingIds);
                }

                //  Apply restriction to aggregate
                type.RestrictToBranches(existingBranchIds);
            }

            await membershipTypeRepository.AddAsync(type);
            await unitOfWork.SaveChangesAsync();

            return type.Id;
        }
    }
}
