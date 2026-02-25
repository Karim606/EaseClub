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
          return await _context.MembershipApplications.Include(ma => ma.Answers).FirstOrDefaultAsync(ma => ma.Id == id);
        }

        public async Task UpdateAnswerAsync(MembershipApplication application, CancellationToken ct = default)
        {
            // By simply ensuring the Aggregate Root is tracked, EF will see 
            // the new items in the _answers collection.
            // If EF still marks them as 'Modified', force them to 'Added' here:
            foreach (var answer in application.Answers)
            {
                var entry = _context.Entry(answer);
                if (entry.State == EntityState.Detached)
                {
                    await _context.ApplicationAnswers.AddAsync(answer); // Forces 'Added' state
                }
            }
        }
    }
}
