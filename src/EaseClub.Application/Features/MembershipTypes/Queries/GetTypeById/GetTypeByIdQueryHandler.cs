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
    public class GetMembershipTypeByIdQueryHandler(IMembershipTypeRepository repository)
       : IRequestHandler<GetMembershipTypeByIdQuery, Result<MembershipTypeDto>>
    {
        public async Task<Result<MembershipTypeDto>> Handle(GetMembershipTypeByIdQuery request, CancellationToken ct)
        {
            var type = await repository.GetByIdAsync(request.MembershipTypeId, ct);
            if (type is null)
                return Error.NotFound("Membership type not found.");


            return new MembershipTypeDto(
                type.Id,
                type.Name,
                type.Description,
                type.AllBranchesPermitted
            );
        }
    }
}