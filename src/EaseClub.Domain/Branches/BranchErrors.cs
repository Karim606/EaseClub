using EaseClub.Domain.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Branches
{
    public static class BranchErrors
    {
        public static readonly Error Name_Length_NotSuitable =  Error.Validation
            ("Branch.Name'sLengthNotSuitable", "Branch name must be between 3 and 100 characters.");

        public static readonly Error NullOrWhiteSpaces = Error.Validation
            ("Branch.Name.NullOrWhiteSpaces", "Branch name cannot be empty or whitespace.");

    }
}
