using EaseClub.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Common
{
    public record OwnershipRule(
    Func<IClubAuthorizationService,Guid, Task<bool>> Check,
    string ResourceName,
    Guid ResourceId);
}
