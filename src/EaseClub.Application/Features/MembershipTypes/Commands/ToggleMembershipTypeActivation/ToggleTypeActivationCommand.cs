using EaseClub.Application.Common;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipTypes;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipTypes.Commands.ToggleMembershipTypeActivation
{
    public record ToggleTypeActivationCommand(Guid ClubId, Guid Id) : IRequest<Result<Success>>, IRequireClubOwnershipValidation
    {
        public IEnumerable<OwnershipRule> Rules()
        {
            yield return new OwnershipRule(
                (auth,clubId) => auth.DoesResourceBelongToClubAsync<MembershipType>(Id,clubId),
                nameof(MembershipType),
                Id);
        }
    }
}
