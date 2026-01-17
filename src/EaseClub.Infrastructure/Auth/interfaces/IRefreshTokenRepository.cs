using EaseClub.Infrastructure.Auth.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Auth.interfaces
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByHashedTokenAsync(string hashedToken);
        Task AddAsync(RefreshToken token);
        Task RevokeAsync(
            RefreshToken token,
            RevokeReasons reason,
            Guid? replacedByTokenId = null);
        Task RevokeAllUserTokensAsync(Guid userId, RevokeReasons reason);
        Task SaveChangesAsync();
    }
}
