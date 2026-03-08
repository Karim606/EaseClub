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

        public async Task<HashSet<Guid>> GetExistingBranchIdsAsync(IEnumerable<Guid> ids)
        {
          return await _context.Branches.Where(b => ids.Contains(b.Id)).Select(b => b.Id).ToHashSetAsync();
        }

        public async Task<bool> IsExistByName(Guid clubId, string name)
        {
            return await _context.Branches.AnyAsync(b => b.ClubId == clubId && b.Name == name);
        }
        public async Task<Branch> GetBranchesByMembershipType(Guid typeId,CancellationToken ct = default)
        {
            return await _context.MembershipTypeBranches.Where(b => b.MembershipType.Id == typeId)
                .Select(b => b.Branch).FirstOrDefaultAsync(ct);
                
        }

    }
}
