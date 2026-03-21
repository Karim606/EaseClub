using EaseClub.Domain.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Branches
{
    public interface IBranchRepository:IRepository<Branch>
    {
        public Task<List<Branch>> GetBranchesByClubIdAsync(Guid clubId,bool?Active,CancellationToken cancellationToken=default);
        public Task<bool> IsExistByName(Guid clubId,string name);
        public Task<HashSet<Guid>> GetExistingBranchIdsAsync(IEnumerable<Guid> ids);

        public Task<List<Branch>> GetBranchesByMembershipType(Guid typeId, CancellationToken ct = default);
    }
}
