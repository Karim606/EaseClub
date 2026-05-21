using EaseClub.Application.Common.Pagination;
using EaseClub.Application.Features.Payment.Queries.GetInvoicesForClub;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Payment.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Payment.Queries
{
    public interface IInvoiceQueryService
    {
        Task<Result<UnifiedPaginatedResponse<InvoiceDto>>> GetInvoicesAsync(Guid? clubId, Guid? userId, InvoiceStatus? status, BillingItemType? billingItemType, string? search, PaginationRequest paginationRequest, CancellationToken ct);
    }

}
