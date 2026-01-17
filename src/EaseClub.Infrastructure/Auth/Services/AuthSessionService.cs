using EaseClub.Application.Common.interfaces;
using EaseClub.Application.Features.Auth.Dtos;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Infrastructure.Auth.Entities;
using EaseClub.Infrastructure.Auth.interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Auth.Services
{
    public class AuthSessionService
    {
        private readonly UserManager<AuthUser> _userManager;
        private readonly IJwtService _jwtService;
        private readonly IRefreshTokenRepository _refreshRepo;
        private readonly ILogger<AuthSessionService> _logger;

        public AuthSessionService(
            UserManager<AuthUser> userManager,
            IJwtService jwtService,
            IRefreshTokenRepository refreshRepo,
            ILogger<AuthSessionService> logger)
        {
            _userManager = userManager;
            _jwtService = jwtService;
            _refreshRepo = refreshRepo;
            _logger = logger;
        }

        public async Task<Result<AuthTokensDto>> LoginAsync(string email, string password, string ip, string deviceInfo)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning("Login failed. User not found: {Email}", email);
                return Error.NotFound("User not found");
            }

            if (!await _userManager.CheckPasswordAsync(user, password))
            {
                _logger.LogWarning("Login failed. Invalid password: {Email}", email);
                return Error.Unauthorized("Invalid credentials");
            }

            var roles = (await _userManager.GetRolesAsync(user)).ToList();
            var accessToken = _jwtService.GenerateToken(user.UserName!, user.Email!, user.Id, roles).Value;
            var refresh = _jwtService.GenerateRefreshToken().Value;
            var hashed = _jwtService.HashToken(refresh.UnHashedToken);

            var refreshToken = RefreshToken.Create(Guid.Empty, user.Id, hashed, refresh.ExpiresAt, ip, deviceInfo);

            await _refreshRepo.AddAsync(refreshToken);
            await _refreshRepo.SaveChangesAsync();

            _logger.LogInformation("User logged in successfully: {Email}, IP: {IP}, Device: {Device}", email, ip, deviceInfo);
            return new AuthTokensDto(accessToken, refresh.UnHashedToken);
        }

        public async Task<Result<AuthTokensDto>> RefreshAsync(string refreshToken, string ip)
        {
            var hashed = _jwtService.HashToken(refreshToken);
            var storedToken = await _refreshRepo.GetByHashedTokenAsync(hashed);

            if (storedToken == null)
            {
                _logger.LogWarning("Refresh failed. Invalid token used.");
                return Error.Unauthorized("Invalid refresh token");
            }

            if (storedToken.RevokedAt != null)
            {
                await _refreshRepo.RevokeAllUserTokensAsync(storedToken.UserId, RevokeReasons.Compromised);
                await _refreshRepo.SaveChangesAsync();

                _logger.LogWarning("Refresh token reuse detected. UserId: {UserId}", storedToken.UserId);
                return Error.Unauthorized("Token reuse detected");
            }

            if (storedToken.ExpiresAt < DateTime.UtcNow)
            {
                await _refreshRepo.RevokeAsync(storedToken, RevokeReasons.Expired);
                await _refreshRepo.SaveChangesAsync();

                _logger.LogWarning("Expired refresh token used. UserId: {UserId}", storedToken.UserId);
                return Error.Unauthorized("Refresh token expired");
            }

            var user = await _userManager.FindByIdAsync(storedToken.UserId.ToString());
            if (user == null)
            {
                _logger.LogWarning("Refresh failed. User not found: {UserId}", storedToken.UserId);
                return Error.NotFound("User not found");
            }

            // rotate token
            var newRefresh = _jwtService.GenerateRefreshToken().Value;
            var newHashed = _jwtService.HashToken(newRefresh.UnHashedToken);
            var newToken = RefreshToken.Create(Guid.Empty, user.Id, newHashed, newRefresh.ExpiresAt, ip, storedToken.DeviceInfo);

            await _refreshRepo.RevokeAsync(storedToken, RevokeReasons.ReplacedByNewToken, newToken.Id);
            await _refreshRepo.AddAsync(newToken);
            await _refreshRepo.SaveChangesAsync();

            var roles = (await _userManager.GetRolesAsync(user)).ToList();
            var accessToken = _jwtService.GenerateToken(user.UserName!, user.Email!, user.Id, roles).Value;

            _logger.LogInformation("Refresh token rotated successfully for UserId: {UserId}", user.Id);
            return new AuthTokensDto(accessToken, newRefresh.UnHashedToken);
        }

        public async Task<Result<Success>> LogoutAsync(string refreshToken)
        {
            var hashed = _jwtService.HashToken(refreshToken);
            var token = await _refreshRepo.GetByHashedTokenAsync(hashed);

            if (token == null)
            {
                _logger.LogWarning("Logout attempted with invalid refresh token.");
                return Result.Success;
            }

            await _refreshRepo.RevokeAsync(token, RevokeReasons.RevokedByUser);
            await _refreshRepo.SaveChangesAsync();

            _logger.LogInformation("Refresh token revoked by user. UserId: {UserId}", token.UserId);
            return Result.Success;
        }
    }
}
