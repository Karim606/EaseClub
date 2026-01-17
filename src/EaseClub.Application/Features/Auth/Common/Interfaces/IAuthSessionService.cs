using EaseClub.Application.Features.Auth.Dtos;
using EaseClub.Domain.Common.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Auth.Common.Interfaces
{
    public interface IAuthSessionService
    {
        Task<Result<AuthTokensDto>> LoginAsync(string email, string password, string ip, string deviceInfo);
        Task<Result<Success>> LogoutAsync(string refreshToken);
        Task<Result<AuthTokensDto>> RefreshAsync(string refreshToken, string ip);
    }
}
