using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Branches.Commands.CreateBranch
{
    public record CreateBranchCommand(Guid ClubId, string Name) : IRequest<Result<Guid>>, IRequireClubAdmin;
}
