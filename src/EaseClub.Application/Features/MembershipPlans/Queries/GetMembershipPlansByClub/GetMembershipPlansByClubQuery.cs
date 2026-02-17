using EaseClub.Application.Common.Pagination.Parameters;
using EaseClub.Application.Common.Pagination.Results;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipPlans.Queries.GetMembershipPlansByClub
{
    public record GetMembershipPlansByClubQuery(
     Guid ClubId,
     OffsetPaginationParameters Parameters) : IRequest<Result<OffsetPaginatedResult<MembershipPlanDto>>>;
}
