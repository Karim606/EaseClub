using EaseClub.Domain.PricingPolices;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Data.Repositories
{
    public class PricingPolicyRepository : EfRepository<PricingPolicy>, IPricingPolicyRepository
    {
        public PricingPolicyRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<PricingPolicy>> GetByClubIdAsync(Guid clubId, CancellationToken ct = default)
        {
            return await _context.PricingPolicies.Where(p => p.ClubId == clubId).ToListAsync(ct);
        }

        public async Task<List<PricingPolicy>> GetPoliciesByIdAsync(List<Guid> ids, CancellationToken ct = default)
        {
            return await _context.PricingPolicies.Where(p => ids.Contains(p.Id)).ToListAsync(ct);
        }

        public async Task<List<PricingPolicyAssignment>> GetPricingPolicyAssignmentsByPolicyIdAsync(Guid policyId, CancellationToken ct = default)
        {
            return await _context.PricingPolicyAssignments.Where(p => p.PolicyId == policyId).ToListAsync(ct);
        }

        public async Task<List<PricingPolicyAssignment>> GetPricingPolicyAssignmentsByTargetIdAsync(Guid targetId, CancellationToken ct = default)
        {
            return await _context.PricingPolicyAssignments.Where(p => p.TargetId == targetId).Include(p => p.Policy).ToListAsync(ct);
        }

        /// <inheritdoc />
        public async Task<PricingPolicyAssignment?> GetAssignmentAsync(Guid targetId, Guid policyId, CancellationToken ct = default)
        {
            // Query directly on the DbSet — this finds orphaned rows regardless of
            // whether the ApplicationTemplateDefinitionId shadow FK is NULL.
            return await _context.PricingPolicyAssignments
                .FirstOrDefaultAsync(a => a.TargetId == targetId && a.PolicyId == policyId, ct);
        }

        /// <inheritdoc />
        public Task DeleteAssignmentAsync(PricingPolicyAssignment assignment, CancellationToken ct = default)
        {
            _context.PricingPolicyAssignments.Remove(assignment);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task AddAssignmentAsync(PricingPolicyAssignment assignment, CancellationToken ct = default)
        {
            await _context.PricingPolicyAssignments.AddAsync(assignment, ct);
        }
    }
}
