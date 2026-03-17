using EaseClub.Application.Common.Pagination;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipTypes.Queries.GetMembershipsTypesForMember
{
    public record GetMembershipTypesForMemberQuery(Guid? ClubId,Guid? BranchId,bool? accessToAllBranches,PaginationRequest Pagination) : IRequest<Result<UnifiedPaginatedResponse<MembershipTypeDto>>>;
    
}
