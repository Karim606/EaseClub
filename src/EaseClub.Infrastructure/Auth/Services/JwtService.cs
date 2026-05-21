using EaseClub.Application.Common.interfaces;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Auth.Services
{
    public class JwtService:IJwtService
    {
        private readonly JwtSettings _JwtSettings;
        private readonly ILogger<JwtService> _Logger;
        public JwtService(IOptions<JwtSettings> JwtSettings,ILogger<JwtService>Logger)
        {
            _JwtSettings = JwtSettings.Value;
            _Logger = Logger;
        }
        public Result<string> GenerateToken(string Name, string Email, Guid userId, List<string> Roles)
        {
            try
            {
                var claims = new List<Claim>
                {
                    new Claim("id",userId.ToString() ),
                    new Claim("email",Email),
                    new Claim("name",Name)
                };
                foreach (var role in Roles)
                {
                    claims.Add(new Claim("role", role));
                }
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_JwtSettings.SecretKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                    issuer: _JwtSettings.Issuer,
                    audience: _JwtSettings.Audience,
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(_JwtSettings.Duration),
                    signingCredentials: creds
                    );
                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error generating JWT token for user {UserId}", userId);
                return Error.Failure(description:"failed to generate access token.");
            }
        }

        public Result<(string, DateTime)> GenerateRefreshToken()
        {
            try
            {
                var randomBytes = new byte[64];
                using var rng = RandomNumberGenerator.Create();
                rng.GetBytes(randomBytes);
                var token = Convert.ToBase64String(randomBytes);
                var expiresAt = DateTime.UtcNow.AddDays(_JwtSettings.RefreshTokenExpiryDays);

                return (token,expiresAt);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error generating refresh token");
                return Error.Failure(description: "failed to generate refresh token.");
            }
        }
        public string HashToken(string token)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(hash);
        }
    }
}
