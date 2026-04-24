using Microsoft.Extensions.Logging;
using EaseClub.Application.Features.Branches.Queries.GetBranchesByClub;
using EaseClub.Domain.Branches;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Branches.Queries.GetBranchById
{
    public class GetBranchByIdQueryHandler(IBranchRepository repository,ILogger<GetBranchByIdQueryHandler> logger) : IRequestHandler<GetBranchByIdQuery, Result<BranchDto>>
    {

        public async Task<Result<BranchDto>> Handle(GetBranchByIdQuery request, CancellationToken ct)
        {
            var branch = await repository.GetByIdAsync(request.Id, ct);
            if (branch is null) { logger.LogError("NotFound error in GetBranchByIdQueryHandler: {Error}", Error.NotFound("Branch not found.").ToLogObject()); return Error.NotFound("Branch not found."); }
            return new BranchDto(branch.Id, branch.ClubId, branch.Name,branch.Address,branch.CreatedAt);
        }
    }
}
