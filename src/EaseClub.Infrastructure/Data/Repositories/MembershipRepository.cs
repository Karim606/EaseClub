using EaseClub.Domain.MembershipPlans;
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

        public async Task<Membership> GetByIdWithDetailsAsync(Guid membershipId, CancellationToken ct = default)
        {
            return await _context.Memberships
                .Include(m => m.MembershipType)
                .Include(m => m.MembershipPlan)
                .Include(m => m.MembershipCycles)
                .Include(m => m.FamilyMembers)
                .Include(m =>m.MembershipCycles)
                .AsSplitQuery()
                .FirstOrDefaultAsync(x => x.Id == membershipId, ct);
        }

        public async Task<Membership> GetByApplicationIdAsync(Guid applicationId, CancellationToken ct = default)
        {
            return await _context.Memberships.FirstOrDefaultAsync(x => x.MembershipApplicationId == applicationId, ct);
        }

        public async Task<List<Membership>> GetByMemberIdAsync(Guid userId, CancellationToken ct = default)
        {
            return await _context.Memberships.Where(x => x.MemberId == userId).Include(m => m.Club).ToListAsync(ct);
        }
        public async Task<List<MembershipInstallment>> GetInstallmentsForCurrentCycleAsync(
            Guid membershipId,
            CancellationToken ct = default)
        {
            return await _context.MembershipInstallments
          .Where(i => i.MembershipId == membershipId &&
                      i.MembershipCycleId == _context.MembershipCycles
                          .Where(c => c.MembershipId == membershipId)
                          .OrderByDescending(c => c.Period.StartDate)
                          .Select(c => c.Id)
                          .FirstOrDefault())
          .OrderBy(i => i.DueDate)
          .ToListAsync(ct);
        }

    }
}
