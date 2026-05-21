using EaseClub.Application.Common;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipPlans;
using EaseClub.Domain.MembershipTypes;
using MediatR;

namespace EaseClub.Application.Features.Memberships.Commands.StartDirectPayEnrollment
{
    public record StartDirectPayEnrollmentCommand(
        Guid ClubId,
        Guid MembershipTypeId,
        Guid MembershipPlanId,
        Guid? InstallmentTemplateId
        ) : IRequest<Result<EnrollmentPaymentResponse>>, IRequireResourceValidation
    {
        public IEnumerable<OwnershipRule> Rules()
        {
            yield return new OwnershipRule(
                async (auth, _) => await auth.DoesResourceBelongToClubAsync<MembershipType>(MembershipTypeId, ClubId),
                nameof(MembershipType),
                MembershipTypeId);

            yield return new OwnershipRule(
                async (auth, _) => await auth.DoesResourceBelongToClubAsync<MembershipPlan>(MembershipPlanId, ClubId),
                nameof(MembershipPlan),
                MembershipPlanId);

            yield return new OwnershipRule(
                async (auth, _) => await auth.DoesResourceBelongToClubAsync<InstallmentTemplate>(InstallmentTemplateId ?? Guid.Empty, ClubId),
                nameof(InstallmentTemplate),
                InstallmentTemplateId ?? Guid.Empty);
        }
    }

    public record StartDirectPayEnrollmentRequest(
    Guid ClubId,
    Guid MembershipTypeId,
    Guid? InstallmentTemplateId
    );
}
