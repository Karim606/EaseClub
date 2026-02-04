using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipTypes;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipTypes.Queries.GetMembershipTypesByClub
{
    public class GetMembershipTypesByClubQueryHandler(
        IMembershipTypeRepository membershipTypeRepository
        ) : IRequestHandler<GetMembershipTypesByClubQuery, Result<List<MembershipTypeDto>>>
    {
        public async Task<Result<List<MembershipTypeDto>>> Handle(GetMembershipTypesByClubQuery request, CancellationToken cancellationToken)
        {
            var membershipTypes = (await membershipTypeRepository.GetByClubIdAsync(request.ClubId))
                .Select(mt => new MembershipTypeDto
                {
                    Id = mt.Id,
                    Name = mt.Name,
                    Description = mt.Description,
                    FamilyAllowed = mt.FamilyAllowed,
                    MaxFamilyMembers = mt.MaxFamilyMembers,
                    AllBranchesPermitted = mt.AllBranchesPermitted,
                    BranchIds = mt.PermittedBranches.Select(pb => pb.BranchId).ToList()
                })
                .ToList();

            return membershipTypes;
        }
    }
}
