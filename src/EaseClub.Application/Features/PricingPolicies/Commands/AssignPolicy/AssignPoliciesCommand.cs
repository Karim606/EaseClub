using EaseClub.Application.Common;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.ApplicationTemplates;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.PricingPolices;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.PricingPolicies.Commands.AssignPolicy
{
    public record AssignPoliciesCommand(List<PolicyAssignmentDto>Policies, Guid TargetId, PricingPolicyTargetType TargetType) :IRequest<Result<Success>>, IRequireClubOwnershipValidation
    {
        public IEnumerable<OwnershipRule> Rules()
        {
            yield return new OwnershipRule(
               async (auth, clubId) => {
                   foreach (var p in Policies)
                   {
                       var policy = await auth.DoesResourceBelongToClubAsync<PricingPolicy>(p.PolicyId, clubId);
                       if (!policy) return false;
                   }
                   bool target = false;
                   if(TargetType == PricingPolicyTargetType.ApplicationTemplate)
                   {
                      target = await auth.DoesResourceBelongToClubAsync<ApplicationTemplateDefinition>(TargetId, clubId);
                   }
                   return target;
                },
                "PricingPolices&PricingPolicyTarget",
                Guid.Empty
                );
               
        }

    }

    public record PolicyAssignmentDto(
        Guid PolicyId,
        int Priority
    );
}
