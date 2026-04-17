using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Common.Pagination;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Payment.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Payment.Queries.GetInvoicesForClub
{
    public record GetInvoicesByClubQuery(Guid ClubId,
            GetInvoicesFilters filters,
            PaginationRequest paginationRequest):IRequest<Result<UnifiedPaginatedResponse<InvoiceDto>>>,IRequireClubAdmin;



    public record GetInvoicesFilters (
            Guid? userId,
            InvoiceStatus? status,
            BillingItemType? BillingItemType,
            string? search // invoiceId or payableId
    );

    public class InvoiceDto
    {
        public Guid InvoiceId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public Guid BillingItemId { get; set; }
        public BillingItemType BillingItemType { get; set; }
        public decimal Amount { get; set; }
        public InvoiceStatus Status { get; set; }
        public DateTime? PaidAt { get; set; }
        public string? Method { get; set; }
        public string InvoiceReadableId { get; set; } = string.Empty;
        public string ClubName { get; set; } = string.Empty;
    }
}
