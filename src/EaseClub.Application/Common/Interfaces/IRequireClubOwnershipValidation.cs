using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Common.Interfaces
{
    public interface IRequireClubOwnershipValidation:IAuthorizeRequest
    {
        IEnumerable<OwnershipRule> Rules();
    }
}
