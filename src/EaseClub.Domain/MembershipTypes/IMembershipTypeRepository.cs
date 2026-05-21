using EaseClub.Domain.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.MembershipTypes
{
    public interface IMembershipTypeRepository:IRepository<MembershipType>
    {
        public Task<MembershipType> GetByClubIdAndNameAsync(Guid clubId,string Name);

        public Task<List<MembershipType>> GetTypesAsync(Guid? clubId, Guid? branchId, bool? allPermitted, bool? isActive, CancellationToken ct = default);

        public Task<List<MembershipType>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    }
}
