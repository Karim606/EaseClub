using EaseClub.Domain.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.MembershipApplications.Repositories
{
    public interface IMembershipApplicationRepository:IRepository<MembershipApplication>
    {
        public Task<MembershipApplication> GetByIdWithAnswersAsync(Guid id,CancellationToken ct = default);

        public  Task UpdateAnswerAsync(MembershipApplication application, CancellationToken ct = default);
    }
}
