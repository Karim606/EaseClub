using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Branches.Queries.GetBranchesByClub
{
    public sealed class BranchResponse
    {
        public Guid Id { get; init; }
        public Guid ClubId { get; init; }
        public string Name { get; init; }
        public DateOnly CreatedAt { get; init; }
        public string Address { get; init; }
        public BranchResponse(Guid id,Guid clubId, string name, string address, DateOnly createdAt)
        {
            Id = id;
            ClubId = clubId;
            Name = name;
            Address = address;
            CreatedAt = createdAt;
        }
    }
}
