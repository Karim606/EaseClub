using EaseClub.Application.Common.Pagination;
using EaseClub.Application.Common.Pagination.Parameters;
using EaseClub.Application.Common.Pagination.Results;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipPlans.Queries.GetMembershipPlansForMember
{
    public record GetMembershipPlansForMemberQuery(
     Guid? ClubId,
     Guid? MembershipTypeId,
     PaginationRequest pagination
        ) : IRequest<Result<UnifiedPaginatedResponse<MembershipPlanDto>>>;

}
