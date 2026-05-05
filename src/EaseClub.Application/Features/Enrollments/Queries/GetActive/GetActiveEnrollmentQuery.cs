using EaseClub.Application.Common;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using MediatR;

namespace EaseClub.Application.Features.Enrollments.Queries.GetActive
{
    public record ActiveEnrollmentDto(
        Guid Id,
        string ClubName,
        string PlanName,
        decimal TotalPrice,
        decimal AmountToPay,
        DateTime ExpiresAt,
        string Source);

    public record GetActiveEnrollmentQuery(Guid MemberId) : IRequest<Result<ActiveEnrollmentDto?>>, IRequireResourceValidation
    {
        public IEnumerable<OwnershipRule> Rules()
        {
            yield return new OwnershipRule(
                async (auth, _) => await Task.FromResult(auth.IsUserMatch(MemberId)),
                nameof(MemberId),
                MemberId);
        }
    }
}
