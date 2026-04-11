using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipPlans;
using EaseClub.Domain.MembershipPlans.Repositories;
using EaseClub.Domain.MembershipTypes;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipPlans.Command.CreatePlan
{
    public class CreateMembershipPlanHandler(IMembershipPlanRepository membershipPlanRepository,
            IUnitOfWork unitOfWork,IMembershipTypeRepository membershipTypeRepository,
            ILogger<CreateMembershipPlanHandler>logger)
        : IRequestHandler<CreateMembershipPlanCommand, Result<Guid>>
    {

        

        public async Task<Result<Guid>> Handle(CreateMembershipPlanCommand request, CancellationToken cancellationToken)
        {
            var membershipType = await  membershipTypeRepository.GetByIdAsync(request.MembershipTypeId);

            if (membershipType == null)
            {
                logger.LogWarning("Membership type {MembershipTypeId} isnt found ",
                       request.MembershipTypeId);
                return Error.NotFound(description: "Membership type isnt found ");
            }

            // 1. Business Rule: Unique name within the same Club
            if (await membershipPlanRepository.ExistsByNameAsync(request.ClubId, request.Name, cancellationToken))
            {
                logger.LogWarning("Membership plan name {MembershipPlanName} already exists in club {ClubId}",
                    request.Name, request.ClubId);
                return MembershipPlanErrors.NameAlreadyExistsInClub;
            }

            // 2. Create the Domain Entity
            var planResult = MembershipPlan.Create(
                Guid.NewGuid(),
                request.ClubId,
                request.MembershipTypeId,
                request.EnrollmentMode,
                request.ApplicationTemplateId,
                request.subscriptionValidityInYears,
                request.maxFamilyMembers,
                request.Name,
                request.Price,
                request.DurationInDays,
                request.RenewPrice,
                request.InstallmentsAllowedInRenewal
                );

            if (planResult.IsError) { 
                logger.LogWarning("Failed to create membership plan: {Errors}", 
                    string.Join(", ", planResult.Errors.Select(e => e.Description)));
                return planResult.TopError;
            }

            // 3. Persist
            await  membershipPlanRepository.AddAsync(planResult.Value);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return planResult.Value.Id;
        }
    }
}
