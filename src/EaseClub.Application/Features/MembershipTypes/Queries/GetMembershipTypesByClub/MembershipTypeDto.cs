using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipTypes.Queries.GetMembershipTypesByClub
{
    public class MembershipTypeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool FamilyAllowed { get; set; }
        public int? MaxFamilyMembers { get; set; }
        public bool AllBranchesPermitted { get; set; }
        public List<Guid> BranchIds { get; set; }
    }
}
