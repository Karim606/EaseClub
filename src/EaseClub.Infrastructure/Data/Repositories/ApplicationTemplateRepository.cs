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
            // Steps are now a JSON property
            return await _context.ApplicationTemplateDefinitions
                .FirstOrDefaultAsync(at => at.Id == templateId, ct);
        }

        public async Task<ApplicationTemplateDefinition> GetFullTemplateAsync(Guid templateId, CancellationToken ct = default)
        {
            return await _context.ApplicationTemplateDefinitions
                .Include(t => t.ConnectedMembershipPlans)
                .Include(t => t.PricingPolicyAssignments)
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
