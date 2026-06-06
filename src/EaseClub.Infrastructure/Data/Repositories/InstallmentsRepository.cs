using EaseClub.Domain.MembershipPlans;
using EaseClub.Domain.MembershipPlans.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Data.Repositories
{
    public class InstallmentsRepository : EfRepository<InstallmentTemplate>, IInstallmentsTemplatesRepository
    {
        public InstallmentsRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<InstallmentTemplate>> GetByClubIdAsync(Guid clubId, CancellationToken ct)
        {
           return await  _context.InstallmentTemplates
               .Include(x => x.Installments)
               .Where(x => x.ClubId == clubId)
               .AsNoTracking()
               .ToListAsync(ct);
        }

        public async Task<List<InstallmentTemplate>> GetByPlanIdAsync(Guid planId, CancellationToken ct)
        {
            return await _context.InstallmentTemplates
                .Include(x => x.Installments)
                .Where(x => x.MembershipPlans.Any(mp => mp.MembershipPlanId == planId))
                .AsNoTracking()
                .ToListAsync(ct);
        }

        public Task<List<InstallmentTemplate>> GetTemplatesAsync(Guid? clubId, Guid? planId, bool? isActive, CancellationToken ct)
        { 
            var query = _context.InstallmentTemplates
                .Include(x => x.Installments)
                .AsQueryable();
            if (clubId.HasValue) query = query.Where(x => x.ClubId == clubId.Value);
            if (planId.HasValue) query = query.Where(x => x.MembershipPlans.Any(mp => mp.MembershipPlanId == planId.Value));
            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
            return query.AsNoTracking().ToListAsync(ct);
        }

        public async Task<bool> IsTemplateNameExistAsync(Guid clubId, string name)
        {
            return await _context.InstallmentTemplates.AnyAsync(x => x.ClubId == clubId&& x.Name.ToLower() == name.ToLower());
             
        }
    }
}
