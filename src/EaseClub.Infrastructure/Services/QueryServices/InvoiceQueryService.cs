using EaseClub.Application.Common.Pagination;
using EaseClub.Application.Features.Payment.Queries;
using EaseClub.Application.Features.Payment.Queries.GetInvoicesForClub;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipPlans;
using EaseClub.Domain.Payment;
using EaseClub.Domain.Payment.Enums;
using EaseClub.Infrastructure.Common.QueryServices;
using EaseClub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Services.QueryServices
{
    public class InvoiceQueryService : BaseQueryService<Invoice>, IInvoiceQueryService
    {
        public InvoiceQueryService(AppDbContext context, ILogger<InvoiceQueryService> logger)
            : base(context, logger)
        {
        }

        public async Task<Result<UnifiedPaginatedResponse<InvoiceDto>>> GetInvoicesAsync(
            Guid? clubId,
            Guid? userId,
            InvoiceStatus? status,
            BillingItemType? billingItemType,
            string? search, // invoiceId or payableId
            PaginationRequest paginationRequest,
            CancellationToken ct)
        {
            // 1. Base query
            var query = Query();

            if (clubId.HasValue) query = query.Where(i => i.ClubId == clubId.Value);
            if (userId.HasValue) query = query.Where(i => i.MemberId == userId.Value);
            if (status.HasValue) query = query.Where(i => i.Status == status.Value);
            if (billingItemType.HasValue) query = query.Where(i => i.BillingItemType == billingItemType.Value);

            // 2. Search logic
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(i =>
                    i.Id.ToString().Contains(search) ||
                    i.BillingItemReadableId.ToString().Contains(search) ||
                    i.ReadableId.Contains(search));
            }

            // 3. Projection & pagination
            return await GetUnifiedPaginatedAsync<InvoiceDto, DateTime>(
                query,
                paginationRequest,
                selector: i => new InvoiceDto
                {
                    InvoiceId = i.Id,
                    UserName = i.Member != null ? i.Member.FirstName + ' ' + i.Member.LastName : string.Empty,
                    BillingItemId = i.BillingItemId,
                    BillingItemType = i.BillingItemType,
                    Amount = i.Amount,
                    Status = i.Status,
                    InvoiceReadableId = i.ReadableId,
                    ClubName = i.Club != null ? i.Club.Name : string.Empty,
                    PaidAt = i.Transactions
                                .Where(t => t.Status == PaymentTransactionStatus.Succeeded)
                                .Select(t => t.CompletedAt)
                                .FirstOrDefault(),
                    Method = i.Transactions
                                .Where(t => t.Status == PaymentTransactionStatus.Succeeded)
                                .Select(t => t.Method)
                                .FirstOrDefault(),
                    BillingItemReadableId = i.BillingItemReadableId
                },
                orderSelector: i => i.CreatedAt, // default sorting by due date
                cancellationToken: ct
            );
        }

        public async Task<Result<PaymentStatsDto>> GetPaymentStatsAsync(Guid clubId, CancellationToken ct)
        {
            try
            {
                // 1. Total Receivables: Sum of Pending & Overdue installments
                var totalReceivables = await _context.MembershipInstallments
                    .Where(i => i.MembershipCycle.Membership.ClubId == clubId &&
                                (i.Status == InstallmentStatus.Pending || i.Status == InstallmentStatus.Overdue))
                    .SumAsync(i => (decimal?)i.Amount, ct) ?? 0;

                // 2. Overdue Dues & Count
                var overdueQuery = _context.MembershipInstallments
                    .Where(i => i.MembershipCycle.Membership.ClubId == clubId &&
                                i.Status == InstallmentStatus.Overdue);

                var overdueDues = await overdueQuery.SumAsync(i => (decimal?)i.Amount, ct) ?? 0;
                var overdueCount = await overdueQuery.CountAsync(ct);

                // 3. Monthly Revenue: Paid Invoices this calendar month
                var now = DateTime.UtcNow;
                var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

                var monthlyRevenue = await _context.Invoices
                    .Where(i => i.ClubId == clubId && i.Status == InvoiceStatus.Paid)
                    .Where(i => i.Transactions.Any(t => t.Status == PaymentTransactionStatus.Succeeded && t.CompletedAt >= startOfMonth))
                    .SumAsync(i => (decimal?)i.Amount, ct) ?? 0;

                return new PaymentStatsDto
                {
                    TotalReceivables = totalReceivables,
                    OverdueDues = overdueDues,
                    OverdueCount = overdueCount,
                    MonthlyRevenue = monthlyRevenue
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment stats for club {ClubId}", clubId);
                return Error.Failure("Payment.StatsError", "Failed to retrieve payment statistics.");
            }
        }
    }
}
