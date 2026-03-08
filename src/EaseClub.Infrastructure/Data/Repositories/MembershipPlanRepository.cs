using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EaseClub.Domain.MembershipPlans;
using EaseClub.Domain.MembershipPlans.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EaseClub.Infrastructure.Data.Repositories
{
    public class MembershipPlanRepository : EfRepository<MembershipPlan>, IMembershipPlanRepository
    {
        public MembershipPlanRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<bool> ExistsByNameAsync(Guid clubId, string name, CancellationToken cancellationToken = default)
        {
            return await _context.MembershipPlans
                .AnyAsync(mp => mp.ClubId == clubId && mp.Name == name, cancellationToken);
        }

        public async Task<List<MembershipPlan>> GetPlansByClubAsync(Guid clubId,CancellationToken cancellationToken = default)
        {
           return  await _context.MembershipPlans.Where(mp => mp.ClubId == clubId).AsNoTracking().ToListAsync(cancellationToken);
        }

       public async Task<MembershipPlan> GetPlanWithDetailsAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.MembershipPlans.Where(mp => mp.Id == id).Include(mp => mp.InstallmentTemplates)
                .FirstOrDefaultAsync(); 
        }

        public async Task<List<MembershipPlan>> GetPlansByMembershipTypeAsync(Guid typeId, CancellationToken cancellationToken = default)
        {
            return await _context.MembershipPlans.Where(mp => mp.MembershipTypeId == typeId).ToListAsync(cancellationToken);
        }

    }
}
