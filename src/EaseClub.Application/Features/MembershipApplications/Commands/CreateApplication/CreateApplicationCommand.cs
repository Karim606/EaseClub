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

namespace EaseClub.Application.Features.MembershipApplications.Commands.CreateApplication
{
    public record CreateApplicationCommand(
    Guid ClubId,
    Guid TemplateId,
    Guid MembershipTypeId,
    Guid MembershipPlanId,
    Guid InstallmentTemplateId) : IRequest<Result<Guid>>, IRequireClubOwnershipValidation
    {
        public IEnumerable<OwnershipRule> Rules()
        {
            yield return new OwnershipRule(
                    async (auth,_) => await auth.DoesResourceBelongToClubAsync<ApplicationTemplateDefinition>(TemplateId,ClubId),
                    nameof(ApplicationTemplateDefinition),
                    TemplateId
                );

            yield return new OwnershipRule(
                    async (auth, _) => await auth.DoesResourceBelongToClubAsync<MembershipType>(MembershipTypeId, ClubId),
                    nameof(MembershipType),
                    MembershipTypeId
                );

            yield return new OwnershipRule(
                    async (auth, _) => await auth.DoesResourceBelongToClubAsync<MembershipPlan>(MembershipPlanId, ClubId),
                    nameof(MembershipPlan),
                    MembershipPlanId
                );

            yield return new OwnershipRule(
                    async (auth, _) => await auth.DoesResourceBelongToClubAsync<InstallmentTemplate>(InstallmentTemplateId, ClubId),
                    nameof(InstallmentTemplate),
                    InstallmentTemplateId
                );

        }
    }
}
