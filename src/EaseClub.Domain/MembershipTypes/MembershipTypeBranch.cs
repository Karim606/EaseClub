using EaseClub.Domain.Branches;
using EaseClub.Domain.Common.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.MembershipTypes
{
    public class MembershipTypeBranch
    {
        public Guid MembershipTypeId { get; private set; }
        public Guid BranchId { get; private set; }
        public Branch Branch { get; private set; }
        public MembershipType MembershipType { get; private set; }

        private MembershipTypeBranch() { }

        private MembershipTypeBranch(Guid membershipTypeId, Guid branchId)
        {
            MembershipTypeId = membershipTypeId;
            BranchId = branchId;
        }
        public static Result<MembershipTypeBranch> Create(Guid membershipTypeId, Guid branchId)
        {
            if(membershipTypeId == Guid.Empty)
                return MembershipTypeBranchErrors.MembershipTypeIdRequired;

            if (branchId == Guid.Empty)
                    return MembershipTypeBranchErrors.BranchIdRequired;

            return new MembershipTypeBranch(membershipTypeId, branchId);
        }
    }

}
