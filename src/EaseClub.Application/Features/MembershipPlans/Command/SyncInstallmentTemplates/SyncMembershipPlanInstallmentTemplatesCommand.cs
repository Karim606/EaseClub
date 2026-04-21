using EaseClub.Application.Common;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipPlans;
using MediatR;
using System.Text.Json.Serialization;

namespace EaseClub.Application.Features.MembershipPlans.Command.SyncInstallmentTemplates
{
    public record SyncMembershipPlanInstallmentTemplatesCommand(
        List<Guid> InstallmentTemplateIds) : IRequest<Result<Success>>, IRequireClubOwnershipValidation
    {
        [JsonIgnore]
        public Guid PlanId { get; init; }

        public IEnumerable<OwnershipRule> Rules()
        {
            var installmentTemplateIds = InstallmentTemplateIds ?? [];

            yield return new OwnershipRule(
                (auth, clubId) => auth.DoesResourceBelongToClubAsync<MembershipPlan>(PlanId, clubId),
                nameof(MembershipPlan),
                PlanId);

            yield return new OwnershipRule(
                async (auth, clubId) => {
                    foreach (var installmentTemplateId in installmentTemplateIds)
                    {
                        var belongs = await auth.DoesResourceBelongToClubAsync<InstallmentTemplate>(installmentTemplateId, clubId);
                        if (!belongs) return false;
                    }

                    return true;
                },
                nameof(InstallmentTemplate),
                Guid.Empty);
        }
    }
}
