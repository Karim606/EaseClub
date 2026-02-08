using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipPlans.Queries.GetMembershipPlanDetails
{
    public record GetMembershipPlanDetailsQuery(Guid PlanId) : IRequest<Result<MembershipPlanDetailsDto>>;
}
