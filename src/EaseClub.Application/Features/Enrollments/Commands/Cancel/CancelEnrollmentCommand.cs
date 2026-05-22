using EaseClub.Application.Common;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Memberships;
using MediatR;

namespace EaseClub.Application.Features.Enrollments.Commands.Cancel
{
    public record CancelEnrollmentCommand(Guid EnrollmentId) : IRequest<Result<Success>>, IRequireResourceValidation
    {
        public IEnumerable<OwnershipRule> Rules()
        {
             yield return new OwnershipRule(
                async (auth, _) => await auth.DoesResourceBelongToCurrentUserAsync<Enrollment>(EnrollmentId),
                nameof(Enrollment),
                EnrollmentId);
        }
    }
}
