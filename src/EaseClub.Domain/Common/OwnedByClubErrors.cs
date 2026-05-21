using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Common
{
    public static class OwnedByClubErrors
    {
        public static Error ClubIdIsRequired = Error.Validation(code: "ClubId.Is.Required",
            description:"ClubId Is Required And Shouldnt Be Null Or Empty ");
    }
}
