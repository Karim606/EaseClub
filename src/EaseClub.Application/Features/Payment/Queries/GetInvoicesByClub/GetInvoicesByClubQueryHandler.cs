using Microsoft.Extensions.Logging;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Common.Pagination;
using EaseClub.Application.Features.Payment.Queries.GetInvoicesForClub;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Payment.Queries.GetInvoicesByClub
{
    public class GetInvoicesByClubQueryHandler(IInvoiceQueryService queryService,ICurrentUserService currentUserService,
        ILogger<GetInvoicesByClubQueryHandler> logger) : IRequestHandler<GetInvoicesByClubQuery, Result<UnifiedPaginatedResponse<InvoiceDto>>>

    {
        public async Task<Result<UnifiedPaginatedResponse<InvoiceDto>>> Handle(GetInvoicesByClubQuery request, CancellationToken cancellationToken)
        {

            var query = await queryService.GetInvoicesAsync(request.ClubId, request.filters.userId, request.filters.status, request.filters.BillingItemType, request.filters.search,
                request.paginationRequest,cancellationToken);

            return query;
        }
    }
}
