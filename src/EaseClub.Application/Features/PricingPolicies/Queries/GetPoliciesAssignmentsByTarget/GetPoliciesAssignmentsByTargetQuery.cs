using EaseClub.Application.Common;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.ApplicationTemplates;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.PricingPolices;
using EaseClub.Domain.Events;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.PricingPolicies.Queries.GetPoliciesAssignmentsByTarget
{
    public record GetPoliciesAssignmentsByTargetQuery(Guid TargetId,PricingPolicyTargetType TargetType) : IRequest<Result<List<PolicyAssignmentResponse>>>, IRequireClubOwnershipValidation
    {
        public IEnumerable<OwnershipRule> Rules()
        {
            yield return new OwnershipRule(
               async (auth, clubId) => {

                   bool target = false;
                   if (TargetType == PricingPolicyTargetType.ApplicationTemplate)
                   {
                       target = await auth.DoesResourceBelongToClubAsync<ApplicationTemplateDefinition>(TargetId, clubId);
                   }
                   else if (TargetType == PricingPolicyTargetType.Event)
                   {
                       target = await auth.DoesResourceBelongToClubAsync<Event>(TargetId, clubId);
                   }
                   return target;
               },
                "PricingPolicyTarget",
                Guid.Empty
                );
        }
    }

    public class PolicyAssignmentResponse
    {
        public Guid Id { get; set; }
        public Guid PolicyId { get; set; }
        public string PolicyName { get; set; }
        public Guid TargetId { get; set; }
        public PricingPolicyTargetType TargetType { get; set; } 
        public int priority { get; set; }
    }
}
