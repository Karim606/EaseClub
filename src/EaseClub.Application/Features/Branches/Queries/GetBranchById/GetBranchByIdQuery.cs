using EaseClub.Application.Features.Branches.Queries.GetBranchesByClub;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Branches.Queries.GetBranchById
{
    public record GetBranchByIdQuery(Guid Id) : IRequest<Result<BranchDto>>;
}
