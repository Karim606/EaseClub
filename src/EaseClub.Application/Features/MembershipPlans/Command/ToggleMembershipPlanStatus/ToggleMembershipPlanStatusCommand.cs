using EaseClub.Application.Common;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.MembershipPlans;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;

namespace EaseClub.Application.Features.MembershipPlans.Command.ToggleMembershipPlanStatus
{
    public record ToggleMembershipPlanStatusCommand(Guid Id) : IRequest<Result<Success>>, IRequireClubOwnershipValidation
    {
        public IEnumerable<OwnershipRule> Rules()
        {
            yield return new OwnershipRule(
                (auth, clubId) => auth.DoesResourceBelongToClubAsync<MembershipPlan>(Id, clubId),
                nameof(MembershipPlan),
                Id);
        }
    }
}
