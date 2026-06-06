using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Payment.Queries.GetPaymentStatsByClub
{
    public class GetPaymentStatsByClubQueryHandler(IInvoiceQueryService queryService)
        : IRequestHandler<GetPaymentStatsByClubQuery, Result<PaymentStatsDto>>
    {
        public async Task<Result<PaymentStatsDto>> Handle(GetPaymentStatsByClubQuery request, CancellationToken cancellationToken)
        {
            return await queryService.GetPaymentStatsAsync(request.ClubId, cancellationToken);
        }
    }
}
