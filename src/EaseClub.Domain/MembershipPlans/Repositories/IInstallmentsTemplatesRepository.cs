using EaseClub.Domain.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.MembershipPlans.Repositories
{
    public interface IInstallmentsTemplatesRepository:IRepository<InstallmentTemplate>
    {
        public Task<bool> IsTemplateNameExistAsync(Guid clubId, string name);

        public Task<List<InstallmentTemplate>> GetByClubIdAsync(Guid clubId,CancellationToken ct);

        public Task<List<InstallmentTemplate>> GetByPlanIdAsync(Guid planId, CancellationToken ct);

        public Task<List<InstallmentTemplate>>GetTemplatesAsync(Guid?clubId,Guid? planId,bool? isActive,CancellationToken ct);
    }
}
