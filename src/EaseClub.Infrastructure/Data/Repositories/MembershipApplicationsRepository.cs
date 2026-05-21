using EaseClub.Domain.MembershipApplications;
using EaseClub.Domain.MembershipApplications.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Data.Repositories
{
    public class MembershipApplicationsRepository : EfRepository<MembershipApplication>, IMembershipApplicationRepository
    {
        public MembershipApplicationsRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<MembershipApplication> GetByIdWithAnswersAsync(Guid id, CancellationToken ct = default)
        {
            // Answers is now a JSON property, loaded automatically by EF
            return await _context.MembershipApplications.FirstOrDefaultAsync(ma => ma.Id == id, ct);
        }
    }
}
