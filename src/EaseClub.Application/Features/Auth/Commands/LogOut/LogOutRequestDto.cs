using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Auth.Commands.LogOut
{

    public sealed class LogoutRequestDto
    {
        public string? RefreshToken { get; init; }
    }
}
