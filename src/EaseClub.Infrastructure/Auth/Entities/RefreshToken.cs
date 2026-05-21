using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Auth.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public AuthUser User { get; private set; }
        public string Token { get; private set; }
        public DateTime ExpiresAt { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? RevokedAt { get; private set; }
        public Guid? ReplacedByTokenId { get; private set; }
        public RevokeReasons? RevokeReason { get; private set; } = null;
        public RefreshToken ReplacedBy { get; private set; }

        public string CreatedByIp { get; private set; }
        public string DeviceInfo { get; private set; }

        private RefreshToken() { }
        private RefreshToken(Guid id, Guid userId, string token, DateTime expiresAt, string createdByIp, string deviceInfo)
        {
            Id = id;
            this.UserId = userId;
            this.Token = token;
            this.ExpiresAt = expiresAt;
            CreatedAt = DateTime.Now;
            this.CreatedByIp = createdByIp;
            this.DeviceInfo = deviceInfo;

        }

        public static RefreshToken Create(Guid id, Guid userId, string token, DateTime expiresAt, string createdByIp, string deviceInfo)
        {
            id = id == Guid.Empty ? Guid.NewGuid() : id;

            return new RefreshToken(id, userId, token, expiresAt, createdByIp, deviceInfo);

        }

        public void Revoke(Guid? newRefreshTokenId, RevokeReasons reason)
        {
            RevokedAt = DateTime.Now;
            ReplacedByTokenId = newRefreshTokenId;
            RevokeReason = reason;
        }
    }
}
