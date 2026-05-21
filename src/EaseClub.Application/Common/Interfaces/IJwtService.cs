using EaseClub.Domain.Common.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Common.interfaces
{
    public interface IJwtService
    {
        Result<string> GenerateToken(string Name, string Email, Guid Id, List<string> Roles);
        Result<(string UnHashedToken, DateTime ExpiresAt)> GenerateRefreshToken();
        public string HashToken(string token);
    }
}
