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
    public class GetBranchesByClubQueryHandler(IBranchRepository branchRepository,ILogger<GetBranchesByClubQueryHandler>logger)
        :IRequestHandler<GetBranchesByClubQuery, Result<List<BranchResponse>>>
    {
        public async Task<Result<List<BranchResponse>>> Handle(GetBranchesByClubQuery request, CancellationToken cancellationToken)
        {
            var branches = await branchRepository.GetBranchesByClubIdAsync(request.ClubId);
            if(branches.Count==0)
            {
                logger.LogWarning("No branches found for Club with Id {ClubId}", request.ClubId);
                return Error.NotFound(description: $"No branches found for Club with Id {request.ClubId}");
            }
            var response = branches.Select(b => new BranchResponse(b.Id,b.ClubId,b.Name,DateOnly.FromDateTime(b.CreatedAt))).ToList();
            return response;
        }
    }
}
