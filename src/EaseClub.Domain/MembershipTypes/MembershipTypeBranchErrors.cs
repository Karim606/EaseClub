using EaseClub.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.MembershipTypes
{
    public static class MembershipTypeBranchErrors
    {
        public static Error MembershipTypeIdRequired = Error.Validation
            (code: "MembershipTypeBranch.MembershipTypeIdRequired",
            description: "Membership Type ID is required.");

        public static Error BranchIdRequired = Error.Validation(code: "MembershipTypeBranch.BranchIdRequired",
            description: "Branch ID is required.");
    }
}
