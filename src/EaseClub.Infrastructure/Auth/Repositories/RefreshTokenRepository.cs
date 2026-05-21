using EaseClub.Infrastructure.Auth.Entities;
using EaseClub.Infrastructure.Auth.interfaces;
using EaseClub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Auth.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AppDbContext _db;

        public RefreshTokenRepository(AppDbContext db)
        {
            _db = db;
        }

        public Task<RefreshToken?> GetByHashedTokenAsync(string hashedToken)
        {
            return _db.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == hashedToken);
        }

        public async Task AddAsync(RefreshToken token)
        {
            await _db.RefreshTokens.AddAsync(token);
        }

        public Task RevokeAsync(
            RefreshToken token,
            RevokeReasons reason,
            Guid? replacedByTokenId = null)
        {
            token.Revoke(replacedByTokenId, reason);
            return Task.CompletedTask;
        }

        public async Task RevokeAllUserTokensAsync(Guid userId, RevokeReasons reason)
        {
            var tokens = await _db.RefreshTokens
                .Where(x => x.UserId == userId && x.RevokedAt == null)
                .ToListAsync();

            foreach (var token in tokens)
                token.Revoke(null, reason);
        }

        public Task SaveChangesAsync()
        {
            return _db.SaveChangesAsync();
        }
    }
}
