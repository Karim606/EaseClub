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
    public class ApplicationTemplateRepository : EfRepository<ApplicationTemplateDefinition>,IApplicationTemplateRepository
    {
        public ApplicationTemplateRepository(AppDbContext context) : base(context)
        {
        }


        public async Task<ApplicationTemplateDefinition> GetTemplateWithStepsAsync(Guid templateId,CancellationToken ct)
        {
         return await  _context.ApplicationTemplateDefinitions.Where(at => at.Id == templateId)
                    .Include(at => at.Steps)
                    .FirstOrDefaultAsync();
        }

        public async Task<ApplicationTemplateDefinition> GetFullTemplateAsync(Guid templateId, CancellationToken ct = default)
        {
            return await _context.ApplicationTemplateDefinitions
                .Include(t => t.ConnectedMembershipPlans)
                .Include(t => t.Steps)
                .ThenInclude(s => s.Sections)
                .ThenInclude(sec => sec.Fields)
                .AsSplitQuery()
                .FirstOrDefaultAsync(t => t.Id == templateId, ct);
        }

        public async Task<ApplicationTemplateDefinition> GetTemplateWithConnectedMembershipPlansAsync(Guid templateId, CancellationToken ct = default)
        {
            return await _context.ApplicationTemplateDefinitions
                .Include(t => t.ConnectedMembershipPlans)
                .FirstOrDefaultAsync(t => t.Id == templateId, ct);
        }
    }
}
