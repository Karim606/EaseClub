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
            return await _context.PricingPolicies.Where(p => p.ClubId == clubId).ToListAsync();
        }
    }
}
