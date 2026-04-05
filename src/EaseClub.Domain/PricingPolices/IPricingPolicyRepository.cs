using EaseClub.Domain.Common.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.PricingPolices
{
   public interface IPricingPolicyRepository : IRepository<PricingPolicy>
    {
        public Task<List<PricingPolicy>>GetByClubIdAsync(Guid clubId, CancellationToken ct=default);

        public Task<List<PricingPolicyAssignment>>GetPricingPolicyAssignmentsByTargetIdAsync(Guid targetId, CancellationToken ct=default);

        public Task<List<PricingPolicyAssignment>>GetPricingPolicyAssignmentsByPolicyIdAsync(Guid policyId, CancellationToken ct=default);

        public Task<List<PricingPolicy>>GetPoliciesByIdAsync(List<Guid> ids, CancellationToken ct = default);
    }
}
