using EaseClub.Domain.Memberships;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Data.Repositories
{
    public class MembershipRepository : EfRepository<Membership>, IMembershipRepository
    {
        public MembershipRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Membership> GetByApplicationIdAsync(Guid applicationId, CancellationToken ct  = default)
        {
           return await _context.Memberships.FirstOrDefaultAsync(x => x.MembershipApplicationId == applicationId ,ct);
        }

        public async Task<List<Membership>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        {
            return await _context.Memberships.Where(x => x.UserId == userId).Include(m => m.Club).ToListAsync(ct);
        }
    }
}
