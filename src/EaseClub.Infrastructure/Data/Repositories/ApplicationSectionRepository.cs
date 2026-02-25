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
    public class ApplicationSectionRepository : EfRepository<ApplicationSectionDefinition>, IApplicationSectionRepository
    {
        public ApplicationSectionRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<ApplicationSectionDefinition> GetSectionWithFields(Guid sectionId, CancellationToken ct)
        {
          return  await _context.ApplicationSectionDefinitions.Where(sec => sec.Id == sectionId)
                .Include(sec => sec.Fields)
                .FirstOrDefaultAsync();
        }
    }
}
