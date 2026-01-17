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
        public Guid userId { get; private set; }
        public string token { get; private set; }
        public DateTime expiresAt { get; private set; }
        public DateTime createdAt { get; private set; }
        public DateTime? revokedAt { get; private set; }
        public Guid? replacedByTokenId { get; private set; }
        public RevokeReasons? revokeReason { get; private set; } = null;
        public RefreshToken ReplacedBy { get; private set; }

        public string createdByIp { get; private set; }
        public string deviceInfo { get; private set; }

        private RefreshToken() { }
        private RefreshToken(Guid id, Guid userId, string token, DateTime expiresAt, string createdByIp, string deviceInfo)
        {
            Id = id;
            this.userId = userId;
            this.token = token;
            this.expiresAt = expiresAt;
            createdAt = DateTime.Now;
            this.createdByIp = createdByIp;
            this.deviceInfo = deviceInfo;

        }

        public static RefreshToken Create(Guid id, Guid userId, string token, DateTime expiresAt, string createdByIp, string deviceInfo)
        {
            id = id == Guid.Empty ? Guid.NewGuid() : id;

            return new RefreshToken(id, userId, token, expiresAt, createdByIp, deviceInfo);

        }

        public void Revoke(Guid? newRefreshTokenId, RevokeReasons reason)
        {
            revokedAt = DateTime.Now;
            replacedByTokenId = newRefreshTokenId;
            revokeReason = reason;
        }
    }
}
