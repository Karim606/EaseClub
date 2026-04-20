using EaseClub.Application.Common;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Memberships;
using EaseClub.Domain.Memberships.ValueObjects;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EaseClub.Application.Features.Memberships.Queries.GetMembershipDetails.GetMembershipDetailQuery;

namespace EaseClub.Application.Features.Memberships.Queries.GetMembershipDetails
{
    public record GetMembershipDetailQuery(
        Guid MembershipId) : IRequest<Result<MembershipDetailDto>>, IRequireClubOwnershipValidation
    {
        public IEnumerable<OwnershipRule> Rules()
        {
            yield return new OwnershipRule(
                async (authService, clubId) => (await authService.DoesResourceBelongToCurrentUserAsync<Membership>(MembershipId)||await authService.DoesResourceBelongToClubAsync<Membership>(MembershipId,clubId)),
                "Membership",
                MembershipId
            );
        }
    }
    public record MembershipDetailDto(
            Guid Id,
            string MembershipNumber,
            string MembershipType,
            string MembershipPlan,
            DateTime CreatedAt,
            MembershipPeriod CurrentPeriod,
            MembershipStatus Status
        );

    public record FamilyMemberDto(string FullName,
        FamilyRelationship Relationship,
        DateOnly DateOfBirth );

}
