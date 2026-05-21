using EaseClub.Domain.Common.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Auth.Common.Interfaces
{
    public interface IAuthIdentityService
    {
        Task<Result<Success>> ChangePasswordAsync(string email, string currentPassword, string newPassword);
        Task<Result<Guid>> RegisterUserAsync(string email, string password);
        Task DeleteUserAsync(Guid userId);
        Task<Result<Success>> RequestResetPasswordAsync(string email);
        Task<Result<Success>> ResetPasswordAsync(string email, string token, string newPassword);
    }
}
