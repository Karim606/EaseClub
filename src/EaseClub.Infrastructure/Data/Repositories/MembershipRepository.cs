using EaseClub.Domain.MembershipPlans;
using EaseClub.Domain.Memberships;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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
                .AsSplitQuery()
                .FirstOrDefaultAsync(x => x.Id == membershipId, ct);
        }

        public async Task<Membership> GetByApplicationIdAsync(Guid applicationId, CancellationToken ct = default)
        {
            return await _context.Memberships.FirstOrDefaultAsync(x => x.MembershipApplicationId == applicationId, ct);
        }

        public async Task<List<Membership>> GetByMemberIdAsync(Guid userId, MembershipStatus? status, CancellationToken ct = default)
        {
            var query = _context.Memberships.Where(x => x.MemberId == userId);
            if (status.HasValue)
            {
                query = query.Where(x => x.Status == status.Value);
            }
            query = query.Include(m => m.Club)
            .Include(m => m.Member)
            .Include(m => m.MembershipCycles)
            .Include(m => m.FamilyMembers)
            .AsSplitQuery();

            return await query.ToListAsync(ct);
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

        public async Task<Membership?> GetByInstallmentIdAsync(Guid installmentId, CancellationToken ct = default)
        {
            return await _context.Memberships
                .Include(m => m.MembershipCycles)
                    .ThenInclude(c => c.Installments)
                .FirstOrDefaultAsync(m => m.MembershipCycles
                    .SelectMany(c => c.Installments)
                    .Any(i => i.Id == installmentId), ct);
        }

        public async Task<Membership?> GetByMemberAndTypeAsync(Guid memberId, Guid typeId, CancellationToken ct = default)
        {
            return await _context.Memberships
                .Include(m => m.MembershipType)
                .FirstOrDefaultAsync(m => m.MemberId == memberId && m.MembershipTypeId == typeId, ct);
        }

        public async Task<MembershipInstallment?> GetInstallmentByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.MembershipInstallments.FirstOrDefaultAsync(x => x.Id == id, ct);
        }
    }
}
