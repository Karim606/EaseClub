using EaseClub.Application.Common;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.MembershipTypes;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipPlans.Command.CreatePlan
{
    public record CreateMembershipPlanCommand(
    Guid ClubId,
    Guid MembershipTypeId,
    string Name,
    decimal Price,
    int DurationInDays,
    int subscriptionValidityInYears,
    int maxFamilyMembers
    ) : IRequest<Result<Guid>>, IRequireClubAdmin, IRequireClubOwnershipValidation
    {
        public IEnumerable<OwnershipRule> Rules()
        {
            yield return new OwnershipRule( async(auth, clubId) => await auth.DoesResourceBelongToClubAsync<MembershipType>(MembershipTypeId, clubId),
                nameof(MembershipType),
                MembershipTypeId
            );
        }

    }
}
