using EaseClub.Domain.Common.Interfaces;
using EaseClub.Domain.MembershipTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.MembershipPlans.Repositories
{
    public interface IMembershipPlanRepository:IRepository<MembershipPlan>
    {
        public Task<bool> ExistsByNameAsync(Guid clubId, string name, CancellationToken cancellationToken=default);

        public Task<List<MembershipPlan>> GetPlansAsync(Guid? clubId,Guid? membershipTypeId,bool? isActive,CancellationToken cancellationToken=default);

        public Task<MembershipPlan> GetPlanWithDetailsAsync(Guid id,CancellationToken cancellationToken=default);

        public Task<List<MembershipPlan>> GetPlansByMembershipTypeAsync(Guid typeId, CancellationToken cancellationToken = default);

        public Task<List<MembershipPlan>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);

    }
}
