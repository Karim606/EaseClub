using EaseClub.Domain.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.ApplicationTemplates.Repositories
{
    public interface IApplicationSectionRepository:IRepository<ApplicationSectionDefinition>
    {
        public Task<ApplicationSectionDefinition> GetSectionWithFields(Guid sectionId, CancellationToken ct);
    }
}
