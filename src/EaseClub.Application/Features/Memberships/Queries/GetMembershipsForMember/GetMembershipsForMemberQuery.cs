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

namespace EaseClub.Application.Features.Memberships.Queries.GetMembershipsForMember
{
    public record GetMembershipsForMemberQuery(Guid userId, MembershipStatus? Status) : IRequest<Result<List<MembershipForMemberDto>>>, IRequireResourceValidation
    {
        public IEnumerable<OwnershipRule> Rules()
        {
            yield return new OwnershipRule(
                 async (authService, _) => await Task.FromResult(authService.IsUserMatch(userId)),
                 "Memberships",
                 userId
             );
        }
    }

    public record MembershipForMemberDto(Guid Id, string MembershipNumber,Guid ClubId,string ClubName, MembershipStatus Status,MembershipPeriod period, DateTime FarestEndDate);
}
