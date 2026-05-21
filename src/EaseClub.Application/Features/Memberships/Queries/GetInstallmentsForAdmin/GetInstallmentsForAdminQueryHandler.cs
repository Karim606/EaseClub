using Microsoft.Extensions.Logging;
using EaseClub.Application.Common.Pagination;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Memberships.Queries.GetInstallmentsForAdmin
{
    public class GetInstallmentsForAdminQueryHandler(IMembershipInstallmentQueryService membershipInstallmentQueryService,
        ILogger<GetInstallmentsForAdminQueryHandler> logger) : IRequestHandler<GetInstallmentsForAdminQuery, Result<UnifiedPaginatedResponse<InstallmentAdminDto>>>
    {
        public async Task<Result<UnifiedPaginatedResponse<InstallmentAdminDto>>> Handle(GetInstallmentsForAdminQuery request, CancellationToken cancellationToken)
        {
            var installments = await membershipInstallmentQueryService.GetInstallmentsForAdminAsync(
                request.ClubId,
                request.Status,
                request.Search,
                request.PaginationRequest,
                cancellationToken);

            return installments;
        }
    }
}
