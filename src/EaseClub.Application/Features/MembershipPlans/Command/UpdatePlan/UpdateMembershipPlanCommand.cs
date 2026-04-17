using EaseClub.Application.Common;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipPlans;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipPlans.Command.UpdatePlan
{
    public record UpdateMembershipPlanCommand(
    string Name,
    string? Description,
    decimal TotalPrice,
    decimal RenewPrice,
    List<Guid> InstallmentTemplateIds
        ) : IRequest<Result<Success>>, IRequireClubOwnershipValidation
    {
        [JsonIgnore]
        public Guid PlanId { get; init; }
        public IEnumerable<OwnershipRule> Rules()
        {
            yield return new OwnershipRule(
                (auth, clubId) => auth.DoesResourceBelongToClubAsync<MembershipPlan>(PlanId, clubId),
                nameof(MembershipPlan),
                PlanId);

            yield return new OwnershipRule(
                async (auth, clubId) => {
                    foreach (var installmentTemplateId in InstallmentTemplateIds) {
                       var res = await auth.DoesResourceBelongToClubAsync<InstallmentTemplate>(installmentTemplateId, clubId);
                       if (!res) return false;
                    }
                    return true;
                },
                nameof(InstallmentTemplate),
                PlanId);
        }
    };
}
