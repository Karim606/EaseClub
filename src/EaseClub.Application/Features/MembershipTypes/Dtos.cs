using EaseClub.Application.Features.MembershipPlans.Queries.GetMembershipPlansByClub;
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
        // public List<BranchDto>? Branches { get; set; }

    }

    public class BranchDto
    {
        public BranchDto(Guid id, string name) => (Id, Name) = (id, name);
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class MembershipTypeDetailsDto
    {
        public MembershipTypeDetailsDto(Guid id, string name, string? description,
             bool allBranchesPermitted,List<BranchDto>? branches, List<MembershipPlanDto>? membershipPlans)
        {
            Id = id;
            Name = name;
            Description = description;
            AllBranchesPermitted = allBranchesPermitted;
            Branches = branches;
            MembershipPlans = membershipPlans;
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool FamilyAllowed { get; set; }
        public int? MaxFamilyMembers { get; set; }
        public bool AllBranchesPermitted { get; set; }
        public List<BranchDto>? Branches { get; set; }
        public List<MembershipPlanDto>? MembershipPlans { get; set; }

    }

}
