using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Features.Payment;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Payment;
using EaseClub.Domain.Payment.Repositories;
using EaseClub.Domain.Payment.Errors;
using System.Threading;
using System.Threading.Tasks;
using EaseClub.Domain.Common;
using MediatR;

namespace EaseClub.Application.Features.Payment.Commands.ProcessDirectPayment
{
    public record ProcessDirectPaymentCommand(
        Guid InvoiceId,
        CardDetailsDto CardDetails
    ) : IRequest<Result<DirectPaymentResult>>;

    public class ProcessDirectPaymentCommandHandler : IRequestHandler<ProcessDirectPaymentCommand, Result<DirectPaymentResult>>
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IPaymentTransactionRepository _transactionRepository;
        private readonly IPaymentGateway _paymentGateway;

        public ProcessDirectPaymentCommandHandler(
            IInvoiceRepository invoiceRepository,
            IPaymentTransactionRepository transactionRepository,
            IPaymentGateway paymentGateway)
        {
            _invoiceRepository = invoiceRepository;
            _transactionRepository = transactionRepository;
            _paymentGateway = paymentGateway;
        }

        public async Task<Result<DirectPaymentResult>> Handle(ProcessDirectPaymentCommand request, CancellationToken ct)
        {
            var invoice = await _invoiceRepository.GetInvoiceWithTransactions(request.InvoiceId, ct);
            if (invoice == null)
                return Error.NotFound(description: "Invoice not found.");

            // Record Attempt in the Invoice Domain
            var recordResult = invoice.RecordAttempt("Geidea");
            if (!recordResult.IsSuccess)
                return recordResult.TopError;

            var transaction = recordResult.Value;
            await _transactionRepository.AddAsync(transaction, ct);

            // Process payment via gateway
            var result = await _paymentGateway.ProcessPaymentAsync(transaction, request.CardDetails, ct);

            if (result.IsSuccess)
            {
                if (result.Value.Status == "Success")
                {
                    invoice.ConfirmPayment(transaction.Id, DateTime.UtcNow);
                    transaction.AttachExternalRef(result.Value.OrderId ?? "");
                }
                else if (result.Value.Status == "RequiresAction")
                {
                    transaction.AttachExternalRef(result.Value.OrderId ?? "");
                }
            }
            else
            {
                invoice.FailPayment(transaction.Id, result.TopError.Description);
            }

            await _invoiceRepository.UpdateAsync(invoice, ct);

            return result;
        }
    }
}
