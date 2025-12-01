using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Common
{
    public enum ErrorKind
    {
        NotFound = 404,
        Validation = 400,
        Conflict = 409,
        Unexpected = 500,
        Unauthorized = 401,
        Forbidden = 403,
        Failure = 422

    }
}
