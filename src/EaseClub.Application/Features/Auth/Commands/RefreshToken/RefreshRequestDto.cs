using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Auth.Commands.RefreshToken
{
    public sealed class RefreshRequestDto
    {
        public string? RefreshToken { get; init; }
    }
}
