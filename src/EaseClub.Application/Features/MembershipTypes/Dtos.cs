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

    public record MembershipTypeDetails {

        public MembershipTypeDetails(Guid id, string name, string? description, bool allBranchesPermitted, List<BranchesDto> accessedBranches)
        {
            Id = id;
            Name = name;
            Description = description;
            AllBranchesPermitted = allBranchesPermitted;
            AccessedBranches = accessedBranches;
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool AllBranchesPermitted { get; set; }
        public List<BranchesDto> AccessedBranches { get; set; } = new List<BranchesDto>(); 
    }

    public record BranchesDto {

        public BranchesDto(Guid id, string name)
        {

            Id = id;
            Name = name;

        }

        public Guid Id { get; set; }
        public string Name { get; set; }

    };

    

}
