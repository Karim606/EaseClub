using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Payment.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Payment.Queries.GetInvoiceStatus
{
    public class GetInvoiceStatusQueryHandler(
        IInvoiceRepository invoiceRepository,
        ILogger<GetInvoiceStatusQueryHandler> logger)
        : IRequestHandler<GetInvoiceStatusQuery, Result<InvoiceStatusDto>>
    {
        public async Task<Result<InvoiceStatusDto>> Handle(GetInvoiceStatusQuery request, CancellationToken ct)
        {
            var invoice = await invoiceRepository.GetByIdAsync(request.InvoiceId);
            
            if (invoice == null)
            {
                logger.LogWarning("Invoice {Id} not found", request.InvoiceId);
                return Error.NotFound("Invoice not found");
            }

            return new InvoiceStatusDto(
                invoice.Id,
                invoice.Status.ToString(),
                invoice.ReadableId);
        }
    }
}
