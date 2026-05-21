using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Auth.Commands.Login
{
    public sealed class LoginUserDto
    {
        public string Email { get; init; } = default!;
        public string Password { get; init; } = default!;
    }
}
