using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Common.Interfaces
{
    public interface ICurrentUserService
    {
        public string GetId();
        public string? GetEmail();
        public string GetUserName();
        public List<string> GetRoles();

    }
}
