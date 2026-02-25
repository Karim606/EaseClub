using EaseClub.Domain.ApplicationTemplates;
using EaseClub.Domain.ApplicationTemplates.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Data.Repositories
{
    public class ApplicationStepRepository : EfRepository<ApplicationStepDefinition>, IApplicationStepRepository
    {
        public ApplicationStepRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<ApplicationStepDefinition> GetStepWithSections(Guid stepId,CancellationToken ct)
        {
            return await _context.ApplicationStepDefinitions.Where(st => st.Id == stepId)
                 .Include(st => st.Sections)
                 .FirstOrDefaultAsync();
        }
    }
}
