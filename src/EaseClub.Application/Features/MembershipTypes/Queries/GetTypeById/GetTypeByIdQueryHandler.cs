using EaseClub.Application.Features.MembershipPlans.Queries.GetMembershipPlansByClub;
using EaseClub.Application.Features.MembershipTypes.Queries.GetMembershipTypesByClub;
using EaseClub.Domain.Branches;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipPlans.Repositories;
using EaseClub.Domain.MembershipTypes;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipTypes.Queries.GetTypeById
{
    public class GetMembershipTypeByIdQueryHandler(IMembershipTypeRepository repository,
        IMembershipPlanRepository planRepository,
        IBranchRepository branchRepository)
       : IRequestHandler<GetMembershipTypeByIdQuery, Result<MembershipTypeDetailsDto>>
    {
        public async Task<Result<MembershipTypeDetailsDto>> Handle(GetMembershipTypeByIdQuery request, CancellationToken ct)
        {
            var type = await repository.GetByIdAsync(request.MembershipTypeId, ct);
            if (type is null)
                return Error.NotFound("Membership type not found.");

            var plans = await planRepository.GetPlansByMembershipTypeAsync(type.Id, ct);

            var permittedBranches = branchRepository.GetBranchesByMembershipType(type.Id, ct);
            return new MembershipTypeDetailsDto(
                type.Id,
                type.Name,
                type.Description,
                type.AllBranchesPermitted,
                type.PermittedBranches.Select(pb => new BranchDto(pb.BranchId, pb.Branch.Name)).ToList(),
                plans.Select(p => new MembershipPlanDto(p.Id, p.Name, p.MaxPaymentPeriodInDays,p.TotalPrice,p.Description)).ToList()
            );
        }
    }
}
