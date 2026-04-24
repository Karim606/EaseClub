using Microsoft.Extensions.Logging;
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
       IBranchRepository branchRepository,
        ILogger<GetMembershipTypeByIdQueryHandler> logger)
       : IRequestHandler<GetMembershipTypeByIdQuery, Result<MembershipTypeDetails>>
    {
        public async Task<Result<MembershipTypeDetails>> Handle(GetMembershipTypeByIdQuery request, CancellationToken ct)
        {
            var type = await repository.GetByIdAsync(request.MembershipTypeId, ct);

            if (type is null) { logger.LogError("NotFound error in GetMembershipTypeByIdQueryHandler: {Error}", Error.NotFound("Membership type not found.").ToLogObject()); return Error.NotFound("Membership type not found."); }
            var branches = await branchRepository.GetBranchesByMembershipType(type.Id, ct);

            return new MembershipTypeDetails(
                type.Id,
                type.Name,
                type.Description,
                type.AllBranchesPermitted,
                branches.Select(b => new BranchesDto(b.Id,b.Name)).ToList()
            );
        }
    }
}