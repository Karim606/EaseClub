using EaseClub.Domain.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Clubs
{
    public static class ClubErrors
    {
        public static readonly Error Name_Length_NotSuitable = Error.Validation
            ("Club.Name'sLengthNotSuitable", "Club name must be between 3 and 100 characters.");

        public static readonly Error NullOrWhiteSpaces = Error.Validation
            ("Club.Name.NullOrWhiteSpaces", "Club name cannot be empty or whitespace.");

    }
}
