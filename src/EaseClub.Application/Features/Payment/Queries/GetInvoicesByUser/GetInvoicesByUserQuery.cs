using EaseClub.Application.Common;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Common.Pagination;
using EaseClub.Application.Features.Payment.Queries.GetInvoicesForClub;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Payment.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Payment.Queries.GetInvoicesByUser
{
    public record GetInvoicesByUserQuery(Guid UserId,
            GetInvoicesByUserFilters filters,
            PaginationRequest paginationRequest) : IRequest<Result<UnifiedPaginatedResponse<InvoiceDto>>>;
    public record GetInvoicesByUserFilters(
            Guid? ClubId,
            InvoiceStatus? status,
            BillingItemType? BillingItemType,
            string? search // invoiceId or payableId
    );
}
