using EaseClub.Application.Common;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.ApplicationTemplates;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipPlans;
using EaseClub.Domain.MembershipTypes;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.ApplicationTemplates.Commands.Template.SyncMembershipTypes
{
    public record SyncTemplateMembershipPlansCommand(
        Guid TemplateId,
        List<Guid> MembershipPlansIds
        ) : IRequest<Result<Success>>, IRequireClubOwnershipValidation
    {
        public IEnumerable<OwnershipRule> Rules()
        {
            yield return new OwnershipRule(
                async (auth, clubId) => await auth.DoesResourceBelongToClubAsync<ApplicationTemplateDefinition>(TemplateId, clubId),
                nameof(ApplicationTemplateDefinition),
                TemplateId);

            yield return new OwnershipRule(
                async (auth, clubId) => {
                    foreach (var id in MembershipPlansIds) {
                       var res = await auth.DoesResourceBelongToClubAsync<MembershipPlan>(id, clubId);
                        if (!res) return false;
                    }
                    return true;
                },
                nameof(MembershipPlan),
                Guid.Empty);
        }
    }
}
