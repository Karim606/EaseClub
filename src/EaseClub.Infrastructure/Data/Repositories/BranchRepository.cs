using EaseClub.Domain.Branches;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Data.Repositories
{
    public class BranchRepository:EfRepository<Branch>, IBranchRepository
    {
        public BranchRepository(AppDbContext dbContext) : base(dbContext) { }

        public async Task<List<Branch>> GetBranchesByClubIdAsync(Guid clubId)
        {
            return await _context.Branches.Where(b => b.ClubId == clubId).AsNoTracking().ToListAsync();
        }
    }
}
