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
    public class GetBranchByIdQueryHandler : IRequestHandler<GetBranchByIdQuery, Result<BranchDto>>
    {
        private readonly IBranchRepository _repository;

        public GetBranchByIdQueryHandler(IBranchRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<BranchDto>> Handle(GetBranchByIdQuery request, CancellationToken ct)
        {
            var branch = await _repository.GetByIdAsync(request.Id, ct);
            if (branch is null) return Error.NotFound("Branch not found.");

            return new BranchDto(branch.Id, branch.ClubId, branch.Name,branch.CreatedAt);
        }
    }
}
