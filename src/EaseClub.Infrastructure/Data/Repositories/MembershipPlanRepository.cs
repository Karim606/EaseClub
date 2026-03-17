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

        public async Task<List<MembershipPlan>> GetPlansAsync(Guid? clubId,Guid? membershipTypeId,bool? isActive,CancellationToken cancellationToken = default)
        {
            var query = _context.MembershipPlans.AsQueryable();

            if (clubId.HasValue) query = query.Where(mp => mp.ClubId == clubId.Value);
            if (membershipTypeId.HasValue) query = query.Where(mp => mp.MembershipTypeId == membershipTypeId.Value);
            if (isActive.HasValue) query = query.Where(mp => mp.IsActive == isActive.Value); 

            return await query.ToListAsync(cancellationToken);
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

        public async Task<List<MembershipPlan>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
        {
            return await _context.MembershipPlans
               .Where(x => ids.Contains(x.Id))
               .ToListAsync(ct);
        }
    }
}
