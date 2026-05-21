using Microsoft.Extensions.Logging;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.PricingPolices;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.PricingPolicies.Queries.GetPoliciesAssignmentsByTarget
{
    public class GetPoliciesAssignmentsByTargetQueryHandler(IPricingPolicyRepository pricingPolicyRepository,
        ILogger<GetPoliciesAssignmentsByTargetQueryHandler> logger) : IRequestHandler<GetPoliciesAssignmentsByTargetQuery, Result<List<PolicyAssignmentResponse>>>
    {
        public async Task<Result<List<PolicyAssignmentResponse>>> Handle(GetPoliciesAssignmentsByTargetQuery request, CancellationToken cancellationToken)
        {
            var assignments = await pricingPolicyRepository.GetPricingPolicyAssignmentsByTargetIdAsync(request.TargetId, cancellationToken);
               
                if (assignments.Count == 0)
                {
                   return new List<PolicyAssignmentResponse>();
                }
            return assignments.Select(a => new PolicyAssignmentResponse
                {
                    Id = a.Id,
                    PolicyId = a.PolicyId,
                    PolicyName = a.Policy.Name,
                    TargetId = a.TargetId,
                    TargetType = a.TargetType
                }).ToList();
        }
    }
}
