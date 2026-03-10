using EaseClub.Domain.Common;
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
        Task<bool> DoesResourceBelongToClubAsync<TEntity>(Guid entityId, Guid clubId) where TEntity : Entity, IHaveClub;
        Task<bool> DoesResourceBelongToUserAsync<TEntity>(
            Guid entityId,
            Guid userId) where TEntity : Entity, IBelongToUser;

        Task<bool> IsUserAdminOfClubAsync(Guid userId, Guid clubId);
        public  Task<bool> IsUserMemberOfClubAsync(Guid userId, Guid clubId);
        public Task<bool> CheckAppTemplateComponentsOwnership(Type resourceType, Guid resourceId, Guid clubId);
    }
}
