using EaseClub.Domain.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.ApplicationTemplates.Repositories
{
    public interface IApplicationStepRepository:IRepository<ApplicationStepDefinition>
    {
        public Task<ApplicationStepDefinition> GetStepWithSections(Guid stepId,CancellationToken ct);
    }
}
