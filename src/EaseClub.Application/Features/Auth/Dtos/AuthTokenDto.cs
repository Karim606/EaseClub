using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Auth.Dtos
{
    public record AuthTokensDto(string AccessToken, string RefreshToken);

}
