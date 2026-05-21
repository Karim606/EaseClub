using Microsoft.Extensions.Logging;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Common.Pagination;
using EaseClub.Application.Features.Payment.Queries.GetInvoicesForClub;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Payment.Queries.GetInvoicesByUser
{

    public class GetInvoicesByUserQueryHandler(IInvoiceQueryService queryService,ICurrentUserService currentUserService,
        ILogger<GetInvoicesByUserQueryHandler> logger) : IRequestHandler<GetInvoicesByUserQuery, Result<UnifiedPaginatedResponse<InvoiceDto>>>

    {
        public async Task<Result<UnifiedPaginatedResponse<InvoiceDto>>> Handle(GetInvoicesByUserQuery request, CancellationToken cancellationToken)
        {

            var userId = Guid.Parse(currentUserService.GetId());
            var userRoles = currentUserService.GetRoles();
            if (userId != request.UserId&&userRoles.All(r => r!="SuperAdmin"))
            {
                return Error.Unauthorized("You are not authorized to view these invoices.");
            }
            var query = await queryService.GetInvoicesAsync(request.filters.ClubId, request.UserId, request.filters.status, request.filters.BillingItemType, request.filters.search,
                request.paginationRequest, cancellationToken);

            return query;
        }
    }
}
