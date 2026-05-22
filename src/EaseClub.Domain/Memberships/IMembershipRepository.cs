using EaseClub.Domain.Common.Interfaces;
using EaseClub.Domain.MembershipPlans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Memberships
{
    public interface IMembershipRepository:IRepository<Membership>
    {
        public Task<Membership> GetByApplicationIdAsync(Guid applicationId,CancellationToken ct = default);
        public Task<List<Membership>> GetByMemberIdAsync(Guid userId,MembershipStatus? status, CancellationToken ct = default);

        public Task<Membership> GetByIdWithDetailsAsync(Guid membershipId, CancellationToken ct = default);
        public Task<Membership?> GetByInstallmentIdAsync(Guid installmentId, CancellationToken ct = default);
        public Task<List<MembershipInstallment>> GetInstallmentsForCurrentCycleAsync(Guid membershipId, CancellationToken ct = default);
        public Task<Membership?> GetByMemberAndTypeAsync(Guid memberId, Guid typeId, CancellationToken ct = default);
        public Task<MembershipInstallment?> GetInstallmentByIdAsync(Guid id, CancellationToken ct = default);
    }
}
