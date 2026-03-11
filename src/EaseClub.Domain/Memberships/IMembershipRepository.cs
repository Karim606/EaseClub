using EaseClub.Domain.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Memberships
{
    public interface IMembershipRepository:IRepository<Membership>
    {
        public Task<Membership> GetByApplicationIdAsync(Guid applicationId,CancellationToken ct = default);
        public Task<List<Membership>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    }
}
