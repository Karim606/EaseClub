using EaseClub.Domain.Common.Results;
using MediatR;

namespace EaseClub.Application.Features.Enrollments.Queries.GetSummary
{
    public record EnrollmentSummaryDto(
        Guid Id,
        string ClubName,
        string PlanName,
        decimal TotalPrice,
        decimal FirstInstallmentAmount,
        List<InstallmentSummaryDto> Installments,
        DateTime ExpiresAt,
        string Source);

    public record InstallmentSummaryDto(decimal Amount, int DueAfterDays, int Order);

    public record GetEnrollmentSummaryQuery(Guid EnrollmentId) : IRequest<Result<EnrollmentSummaryDto>>;
}
