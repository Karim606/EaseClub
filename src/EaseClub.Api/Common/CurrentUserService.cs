using EaseClub.Application.Common.Interfaces;
using System.Security.Claims;

namespace EaseClub.Api.Common
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly ClaimsPrincipal? _user;
        public CurrentUserService(IHttpContextAccessor? http)
        {
            _user = http?.HttpContext?.User;
            if (_user == null)
            {
                throw new InvalidOperationException("User context is not present");
            }


        }
        public string? GetEmail()
        {
            return _user?.FindFirst("email")?.Value;
        }

        public string GetId()
        {
            var id = _user?.FindFirst("id")?.Value;
            return id;
        }

        public List<string> GetRoles()
        {
            var roles = _user?.FindAll(ClaimTypes.Role)
                       .Select(r => r.Value)
                       .ToList() ?? new List<string>();
            return roles;
        }

        public string GetUserName()
        {
            return _user?.FindFirst("name")?.Value ?? "Unknown";

        }
    }
}
