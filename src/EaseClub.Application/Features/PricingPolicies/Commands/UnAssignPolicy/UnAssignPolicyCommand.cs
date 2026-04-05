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

namespace EaseClub.Application.Features.PricingPolicies.Commands.UnAssignPolicy
{
    public record UnAssignPolicyCommand(Guid PolicyId,Guid TargetId,PricingPolicyTargetType TargetType) : IRequest<Result<Success>>, IRequireClubOwnershipValidation
    {
        public IEnumerable<OwnershipRule> Rules()
        {
            yield return new OwnershipRule(
               async (auth, clubId) => {

                   var policy = await auth.DoesResourceBelongToClubAsync<PricingPolicy>(PolicyId, clubId);
                   bool target = false;
                   if (TargetType == PricingPolicyTargetType.ApplicationTemplate)
                   {
                       target = await auth.DoesResourceBelongToClubAsync<ApplicationTemplateDefinition>(TargetId, clubId);
                   }
                   return policy && target;
               },
                "PricingPolices&PricingPolicyTarget",
                Guid.Empty
                );

        }
    }

}
