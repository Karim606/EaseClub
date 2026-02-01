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
        public Task<List<Branch>> GetBranchesByClubIdAsync(Guid clubId);
    }
}
