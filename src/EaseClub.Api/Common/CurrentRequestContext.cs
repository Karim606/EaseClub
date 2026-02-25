using EaseClub.Application.Common.Interfaces;

namespace EaseClub.Api.Common
{
    public class CurrentRequestContext(IHttpContextAccessor httpContextAccessor) : ICurrentRequestContext
    {
        public string? IpAddress => httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        public string? DeviceInfo => httpContextAccessor?.HttpContext?.Request.Headers["User-Agent"].ToString();
    }
}
