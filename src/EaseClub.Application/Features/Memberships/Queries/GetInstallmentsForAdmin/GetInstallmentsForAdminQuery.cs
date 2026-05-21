using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Common.Pagination;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipPlans;
using EaseClub.Domain.Memberships.ValueObjects;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Memberships.Queries.GetInstallmentsForAdmin
{
    public record GetInstallmentsForAdminQuery(
        Guid ClubId,
        InstallmentStatus? Status,
        string? Search,
        PaginationRequest PaginationRequest) : IRequest<Result<UnifiedPaginatedResponse<InstallmentAdminDto>>>,IRequireClubAdmin;

    public record InstallmentAdminDto(Guid Id, string MembershipNumber, MembershipPeriod MembershipPeriod, decimal Amount, DateTime DueDate, string Status, string ReadableId, Guid? InvoiceId);

}
