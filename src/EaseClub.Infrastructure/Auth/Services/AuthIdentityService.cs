using EaseClub.Application.Common.interfaces;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Features.Auth.Common.Interfaces;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Infrastructure.Auth.Entities;
using EaseClub.Infrastructure.Auth.interfaces;
using EaseClub.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;

namespace EaseClub.Infrastructure.Auth.Services
{
    public class AuthIdentityService: IAuthIdentityService
    {
        private readonly UserManager<AuthUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<AuthUser> _passwordHasher;
        private readonly ILogger<AuthIdentityService> _logger;

        public AuthIdentityService(
            UserManager<AuthUser> userManager,
            IEmailService emailService,
            IConfiguration configuration,
            AppDbContext context,
            IPasswordHasher<AuthUser> passwordHasher,
            ILogger<AuthIdentityService> logger)
        {
            _userManager = userManager;
            _emailService = emailService;
            _configuration = configuration;
            _context = context;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public async Task DeleteUserAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User deleted successfully: {UserId}", userId);
                }
                else
                {
                    _logger.LogError("Failed to delete user {UserId}. Errors: {Errors}", userId, result.Errors);
                }
            }
            else
            {
                _logger.LogWarning("Delete attempt failed. User not found: {UserId}", userId);
            }
        }

        public async Task<Result<Guid>> RegisterUserAsync(string email, string password)
        {
            var normalizedEmail = email.ToUpperInvariant();

            if (await _context.Users.AnyAsync(u => u.NormalizedEmail == normalizedEmail))
                return Error.Conflict("Email already exists");

            var user = new AuthUser
            {
                Id = Guid.NewGuid(),
                Email = email,
                NormalizedEmail = normalizedEmail,
                UserName = email,
                NormalizedUserName = normalizedEmail,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                EmailConfirmed = false
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, password);

            await _context.Users.AddAsync(user);

            var roleId = await _context.Roles
                .Where(r => r.Name == "User")
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            if (roleId != Guid.Empty)
            {
                await _context.UserRoles.AddAsync(new IdentityUserRole<Guid>
                {
                    UserId = user.Id,
                    RoleId = roleId
                });
            }
            
            return user.Id;
        }

        public async Task<Result<Success>> ChangePasswordAsync(string email, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning("Password change failed. User not found: {Email}", email);
                return Error.NotFound("User not found");
            }

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Password change failed for {Email}. Errors: {Errors}", email, result.Errors);
                return Error.Failure("Password change failed");
            }

            _logger.LogInformation("Password changed successfully for {Email}", email);
            return Result.Success;
        }

        public async Task<Result<Success>> RequestResetPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning("Password reset request failed. User not found: {Email}", email);
                return Error.NotFound();
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var resetLink = $"{_configuration["ClientSetting:ClientDomain"]}/auth/reset-password?token={encodedToken}&email={user.Email}";

            var result = await _emailService.SendAsync(email, "Reset-Password", $"Click here to reset your password: {resetLink}.");
            if (result.IsError)
            {
                _logger.LogError("Failed to send password reset email to {Email}", email);
            }
            _logger.LogInformation("Password reset email sent to {Email}", email);

            return result;
        }

        public async Task<Result<Success>> ResetPasswordAsync(string email, string token, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning("Reset password failed. User not found: {Email}", email);
                return Error.NotFound();
            }

            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await _userManager.ResetPasswordAsync(user, decodedToken, newPassword);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Reset password failed for {Email}. Errors: {Errors}", email, result.Errors);
                return Error.Failure();
            }

            _logger.LogInformation("Password reset successfully for {Email}", email);
            return Result.Success;
        }
    }
}
