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

        public async Task<List<Branch>> GetBranchesByClubIdAsync(Guid clubId, bool? Active, CancellationToken cancellationToken = default)
        {
            var query = _context.Branches.AsQueryable().Where(b => b.ClubId == clubId);
            if(Active.HasValue) query =_context.Branches.Where(b => b.ClubId == clubId && b.IsActive == Active.Value);

            return await query.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<HashSet<Guid>> GetExistingBranchIdsAsync(IEnumerable<Guid> ids)
        {
          return await _context.Branches.Where(b => ids.Contains(b.Id)).Select(b => b.Id).ToHashSetAsync();
        }

        public async Task<bool> IsExistByName(Guid clubId, string name)
        {
            return await _context.Branches.AnyAsync(b => b.ClubId == clubId && b.Name == name);
        }
        public async Task<List<Branch>> GetBranchesByMembershipType(Guid typeId,CancellationToken ct = default)
        {
            return await _context.Branches.Where(b => b.MembershipTypeBranchesList.Any(mp => mp.MembershipTypeId == typeId)).ToListAsync(ct);
                
        }

    }
}
