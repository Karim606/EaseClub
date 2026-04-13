using EaseClub.Application.Common.Pagination;
using EaseClub.Application.Features.Payment.Queries;
using EaseClub.Application.Features.Payment.Queries.GetInvoicesForClub;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Payment;
using EaseClub.Domain.Payment.Enums;
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
            if (userId.HasValue) query = query.Where(i => i.UserId == userId.Value);
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
                    UserName = i.User != null ? i.User.FirstName + ' ' + i.User.LastName : string.Empty,
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
                    
                },
                orderSelector: i => i.CreatedAt, // default sorting by due date
                cancellationToken: ct
            );
        }
    }

   
}
