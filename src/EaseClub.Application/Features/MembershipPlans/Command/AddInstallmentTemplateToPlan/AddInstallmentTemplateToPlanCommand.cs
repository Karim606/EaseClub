using EaseClub.Application.Common;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipPlans;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipPlans.Command.AddTemplateToPlan
{
    public record AddInstallmentTemplateToPlanCommand(Guid ClubId, Guid PlanId,
       Guid TemplateId) : IRequest<Result<Success>>, IRequireClubAdmin,IRequireClubOwnershipValidation
    {
        public IEnumerable<OwnershipRule> Rules()
        {
            yield return new OwnershipRule(
                 (auth, clubId) => auth.DoesResourceBelongToClubAsync<MembershipPlan>(PlanId, clubId),
                 nameof(MembershipPlan),
                 PlanId);

            yield return new OwnershipRule(
                (auth, clubId) => auth.DoesResourceBelongToClubAsync<InstallmentTemplate>(TemplateId, clubId),
                nameof(InstallmentTemplate),
                TemplateId);
        }
    }

}
