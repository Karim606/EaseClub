using EaseClub.Application.Common;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Memberships;
using MediatR;

namespace EaseClub.Application.Features.Enrollments.Commands.StartRenewal
{
    public record StartRenewalEnrollmentCommand(
        Guid MembershipId,
        Guid? InstallmentTemplateId) : IRequest<Result<EnrollmentPaymentResponse>>, IRequireResourceValidation
    {
        public IEnumerable<OwnershipRule> Rules()
        {
            yield return new OwnershipRule(
                async (auth, _) => await auth.DoesResourceBelongToCurrentUserAsync<Membership>(MembershipId),
                nameof(Membership),
                MembershipId);
        }
    }
}
