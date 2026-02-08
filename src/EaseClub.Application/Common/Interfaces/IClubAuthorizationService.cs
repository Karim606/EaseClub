using EaseClub.Domain.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Common.Interfaces
{
    public interface IClubAuthorizationService
    {
        Task<bool> DoesResourceBelongToClubAsync<TEntity>(Guid entityId, Guid clubId) where TEntity : class, IHaveClub;
        Task<bool> IsUserAdminOfClubAsync(Guid userId, Guid clubId);
    }
}
