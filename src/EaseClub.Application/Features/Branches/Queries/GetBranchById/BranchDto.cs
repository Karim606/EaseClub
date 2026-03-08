using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Branches.Queries.GetBranchById
{
   
        public sealed class BranchDto
        {
            public Guid Id { get; init; }
            public Guid ClubId { get; init; }
            public string Name { get; init; }
            public DateTime CreatedAt { get; init; }
            
            public BranchDto(Guid id, Guid clubId, string name,DateTime createdAt)
            {
                Id = id;
                ClubId = ClubId;
                Name = name;
                CreatedAt = createdAt;
            }
        }

}
