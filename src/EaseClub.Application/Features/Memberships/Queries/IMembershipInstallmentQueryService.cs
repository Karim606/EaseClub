using EaseClub.Application.Common.Pagination;
using EaseClub.Application.Features.Memberships.Queries.GetInstallmentsForAdmin;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipPlans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Memberships.Queries
{
    public interface IMembershipInstallmentQueryService
    {
        public Task<Result<UnifiedPaginatedResponse<InstallmentAdminDto>>> GetInstallmentsForAdminAsync(Guid clubId, InstallmentStatus? status, string? search, PaginationRequest paginationRequest, CancellationToken cancellationToken = default);
    }
}
