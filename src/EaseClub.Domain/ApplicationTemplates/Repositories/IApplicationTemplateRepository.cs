using EaseClub.Domain.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.ApplicationTemplates.Repositories
{
    public interface IApplicationTemplateRepository:IRepository<ApplicationTemplateDefinition>
    {
        public Task<ApplicationTemplateDefinition> GetTemplateWithStepsAsync(Guid templateId,CancellationToken ct =default);

        public Task<ApplicationTemplateDefinition> GetFullTemplateAsync(Guid templateId, CancellationToken ct = default);

        public Task<ApplicationTemplateDefinition> GetTemplateWithConnectedMembershipPlansAsync(Guid templateId, CancellationToken ct = default);
    }
}
