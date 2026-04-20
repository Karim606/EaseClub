using EaseClub.Application.Features.Branches.Queries.GetBranchesByClub;
using EaseClub.Domain.Branches;
using EaseClub.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Branches.Queries.GetBranchesForAdmins
{
    public class GetBranchesForAdminQueryHandler(IBranchRepository branchRepository)
    : IRequestHandler<GetBranchesForAdminQuery, Result<List<BranchAdminDto>>>
    {
        public async Task<Result<List<BranchAdminDto>>> Handle(GetBranchesForAdminQuery request, CancellationToken cancellationToken)
        {
            var branches = await branchRepository.GetBranchesByClubIdAsync(request.ClubId,request.Active);
            var response = branches.Select(b => new BranchAdminDto{
                Id = b.Id,
                Name = b.Name, 
                ClubId = b.ClubId,
                CreatedAt = DateOnly.FromDateTime(b.CreatedAt),
                Address = b.Address,
                IsActive = b.IsActive}
                ).ToList();
            return response;
        }
    }
}
