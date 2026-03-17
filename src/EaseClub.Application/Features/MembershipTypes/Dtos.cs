using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipTypes
{
    public class MembershipTypeDto
    {
        public MembershipTypeDto(Guid id, string name, string? description, bool allBranchesPermitted)
        {
            Id = id;
            Name = name;
            Description = description;
            AllBranchesPermitted = allBranchesPermitted;
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool AllBranchesPermitted { get; set; }
    }

}
