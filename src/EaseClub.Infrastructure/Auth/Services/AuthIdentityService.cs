using EaseClub.Application.Common.interfaces;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Features.Auth.Common.Interfaces;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Infrastructure.Auth.Entities;
using EaseClub.Infrastructure.Auth.interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
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
        private readonly ILogger<AuthIdentityService> _logger;

        public AuthIdentityService(
            UserManager<AuthUser> userManager,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<AuthIdentityService> logger)
        {
            _userManager = userManager;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<Result<Guid>> RegisterUserAsync(string email, string password)
        {
            var existing = await _userManager.FindByEmailAsync(email);
            if (existing != null)
            {
                _logger.LogWarning("Register attempt failed. Email already exists: {Email}", email);
                return Error.Conflict("Email already exists");
            }

            var user = new AuthUser { UserName = email, Email = email };
            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                _logger.LogError("Failed to create user {Email}. Errors: {Errors}", email, result.Errors);
                return Error.Failure("User creation failed");
            }

            await _userManager.AddToRoleAsync(user, "User");
            _logger.LogInformation("User registered successfully: {Email}", email);

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
