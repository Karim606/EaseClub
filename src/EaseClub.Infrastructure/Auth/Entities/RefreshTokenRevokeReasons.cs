using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Auth.Entities
{
     public enum RevokeReasons
    {
        Expired = 1,
        RevokedByUser = 2,
        ReplacedByNewToken = 3,
        Compromised = 4
    }
}
