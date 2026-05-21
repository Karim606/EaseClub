using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Common.Interfaces
{
    public interface IRequireClubAdmin:IAuthorizeRequest
    {
        Guid ClubId { get; }
    }
}
