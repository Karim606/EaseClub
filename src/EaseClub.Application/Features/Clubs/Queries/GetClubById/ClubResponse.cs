using EaseClub.Domain.Branches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Clubs.Queries.GetClubById
{
    public sealed class ClubResponse
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        
       // public BranchResponse[] Branches { get; init; } = Array.Empty<BranchResponse>();
    }
}
