using EaseClub.Application.Common;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Branches;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;

namespace EaseClub.Application.Features.Branches.Commands.ToggleBranchStatus
{
    public record ToggleBranchStatusCommand(Guid Id) : IRequest<Result<Success>>, IRequireClubOwnershipValidation
    {
        public IEnumerable<OwnershipRule> Rules()
        {
            yield return new OwnershipRule(
                (auth, clubId) => auth.DoesResourceBelongToClubAsync<Branch>(Id, clubId),
                nameof(Branch),
                Id);
        }
    }
}
