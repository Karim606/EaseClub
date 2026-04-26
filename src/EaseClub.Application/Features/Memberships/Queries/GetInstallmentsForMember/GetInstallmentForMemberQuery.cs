using EaseClub.Application.Common;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Memberships;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Memberships.Queries.GetInstallmentsForMember
{
    public record GetInstallmentForMemberQuery(Guid MembershipId) : IRequest<Result<List<InstallmentMemberDto>>>, IRequireResourceValidation
    {
        public IEnumerable<OwnershipRule> Rules()
        {
           yield return new OwnershipRule(
              async (auth,_) => await auth.DoesResourceBelongToCurrentUserAsync<Membership>(MembershipId),
               "Membership",
               MembershipId
                );
        }
    }

    public record InstallmentMemberDto(Guid installmentId, decimal Amount, DateTime DueDate, string Status);
}
