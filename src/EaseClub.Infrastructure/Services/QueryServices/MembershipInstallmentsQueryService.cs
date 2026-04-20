using EaseClub.Application.Common.Pagination;
using EaseClub.Application.Features.Memberships.Queries;
using EaseClub.Application.Features.Memberships.Queries.GetInstallmentsForAdmin;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipPlans;
using EaseClub.Domain.Memberships.ValueObjects;
using EaseClub.Infrastructure.Common.QueryServices;
using EaseClub.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Services.QueryServices
{
    public class MembershipInstallmentsQueryService : BaseQueryService<MembershipInstallment>, IMembershipInstallmentQueryService
    {
        public MembershipInstallmentsQueryService(AppDbContext context, ILogger<MembershipInstallmentsQueryService> logger) : base(context, logger)
        {
        }

        public async Task<Result<UnifiedPaginatedResponse<InstallmentAdminDto>>> GetInstallmentsForAdminAsync(Guid clubId, InstallmentStatus? status, string? search, PaginationRequest paginationRequest, CancellationToken cancellationToken = default)
        {
            var query = Query();

            query = query.Where(i => i.MembershipCycle.Membership.ClubId == clubId);

            if (status.HasValue)
            {
                query = query.Where(i => i.Status == status.Value);
            }
            if (string.IsNullOrEmpty(search))
            {
                query = query.Where(i => i.ReadableId.Contains(search)||
                i.MembershipCycle.Membership.MembershipNumber.Contains(search));

            }

           var res = await GetUnifiedPaginatedAsync<InstallmentAdminDto, DateTime>(
           query,
           paginationRequest,
           selector: i => new InstallmentAdminDto(
               i.Id,
               i.MembershipCycle.Membership.MembershipNumber,
               i.MembershipCycle.Period,
               i.Amount,
               i.DueDate,
               i.Status.ToString(),
               i.ReadableId,
               i.InvoiceId
           ),
           orderSelector: i => i.DueDate, // Most professional to sort by date
           cancellationToken);

            return res;
        }


    }
}



