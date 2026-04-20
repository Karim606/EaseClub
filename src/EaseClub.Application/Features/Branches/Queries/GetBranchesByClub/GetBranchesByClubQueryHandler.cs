using EaseClub.Domain.Branches;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;

using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Branches.Queries.GetBranchesByClub
{
    public class GetBranchesByClubQueryHandler(IBranchRepository branchRepository)
        :IRequestHandler<GetBranchesByClubQuery, Result<List<BranchResponse>>>
    {
        public async Task<Result<List<BranchResponse>>> Handle(GetBranchesByClubQuery request, CancellationToken cancellationToken)
        {
            var branches = await branchRepository.GetBranchesByClubIdAsync(request.ClubId,true);
            var response = branches.Select(b => new BranchResponse(b.Id,b.ClubId,b.Name,b.Address,DateOnly.FromDateTime(b.CreatedAt))).ToList();
            return response;
        }
    }
}
