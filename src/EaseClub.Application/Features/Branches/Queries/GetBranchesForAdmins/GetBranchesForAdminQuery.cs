using EaseClub.Application.Features.Branches.Queries.GetBranchesByClub;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Branches.Queries.GetBranchesForAdmins
{
    public record GetBranchesForAdminQuery(Guid ClubId,bool?Active) : IRequest<Result<List<BranchAdminDto>>>;
    public class BranchAdminDto
    {
        public Guid Id { get; set; }
        public Guid ClubId { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public DateOnly CreatedAt { get; set; }

    }
}
