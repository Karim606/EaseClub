using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Auth.Entities
{
    public class AuthUser:IdentityUser<Guid>
    {
    }
}
