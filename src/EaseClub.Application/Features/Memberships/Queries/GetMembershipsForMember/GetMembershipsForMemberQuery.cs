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
    public record  GetMembershipsForMemberQuery():IRequest<Result<List<MembershipForMemberDto>>>;

        public record MembershipForMemberDto(Guid Id, string MembershipNumber,string ClubName, MembershipStatus Status);
}
