using Microsoft.Extensions.Logging;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Memberships;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Memberships.Queries.GetMembershipDetails
{
    public class GetMembershipDetailQueryHandler(IMembershipRepository membershipRepository,
        ILogger<GetMembershipDetailQueryHandler> logger) : IRequestHandler<GetMembershipDetailQuery, Result<MembershipDetailDto>>
    {
        public async Task<Result<MembershipDetailDto>> Handle(GetMembershipDetailQuery request, CancellationToken cancellationToken)
        {
           var membership = await  membershipRepository.GetByIdWithDetailsAsync(request.MembershipId, cancellationToken);
            if (membership is null) { logger.LogError("NotFound error in GetMembershipDetailQueryHandler: {Error}", Error.NotFound("Membership not found.").ToLogObject()); return Error.NotFound("Membership not found."); }
            var membershipDetailDto = new MembershipDetailDto
            (
                membership.Id,
                membership.MembershipNumber,
                membership.MembershipType.Name,
                membership.MembershipPlan.Name,
                membership.MembershipPlanId,
                membership.MembershipPlan.InstallmentsAllowdInRenewal,
                membership.MembershipPlan.RenewPrice,
                membership.CreatedAt,
                membership.GetCurrentCycle().Period,
                membership.Status,
                membership.FamilyMembers.Select(fm => new FamilyMemberDto
                 (
                     fm.Id,
                     fm.FullName,
                     fm.Relationship,
                     fm.DateOfBirth
                 )).ToList()
            );
            return membershipDetailDto;
        }
    }
}
