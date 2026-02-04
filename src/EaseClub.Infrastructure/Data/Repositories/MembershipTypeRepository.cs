using EaseClub.Domain.MembershipTypes;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Data.Repositories
{
    public class MembershipTypeRepository : EfRepository<MembershipType>, IMembershipTypeRepository
    {
        public MembershipTypeRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<MembershipType> GetByClubIdAndNameAsync(Guid clubId, string name)
        {
            return await _context.MembershipTypes
                .FirstOrDefaultAsync(mt => mt.ClubId == clubId && mt.Name == name);
        }

        public Task<List<MembershipType>> GetByClubIdAsync(Guid clubId)
        {
            return _context.MembershipTypes
                .Where(mt => mt.ClubId == clubId)
                .Include(mt => mt.PermittedBranches)
                .ToListAsync();
        }
    }
}
