using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Features.Payment;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Payment.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Payment.Commands.FinalizeDirectPayment
{
    public record FinalizeDirectPaymentCommand(
        Guid TransactionId,
        string ThreeDSecureId
    ) : IRequest<Result<DirectPaymentResult>>;

    public class FinalizeDirectPaymentCommandHandler : IRequestHandler<FinalizeDirectPaymentCommand, Result<DirectPaymentResult>>
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IPaymentTransactionRepository _transactionRepository;
        private readonly IPaymentGateway _paymentGateway;

        public FinalizeDirectPaymentCommandHandler(
            IInvoiceRepository invoiceRepository,
            IPaymentTransactionRepository transactionRepository,
            IPaymentGateway paymentGateway)
        {
            _invoiceRepository = invoiceRepository;
            _transactionRepository = transactionRepository;
            _paymentGateway = paymentGateway;
        }

        public async Task<Result<DirectPaymentResult>> Handle(FinalizeDirectPaymentCommand request, CancellationToken ct)
        {
            var transaction = await _transactionRepository.GetByIdAsync(request.TransactionId, ct);
            if (transaction == null)
                return Error.NotFound(description: "Transaction not found.");

            var invoice = await _invoiceRepository.GetInvoiceWithTransactions(transaction.InvoiceId, ct);
            if (invoice == null)
                return Error.NotFound(description: "Invoice associated with transaction not found.");

            var result = await _paymentGateway.FinalizePaymentAsync(transaction, request.ThreeDSecureId, ct);

            if (result.IsSuccess && result.Value.Status == "Success")
            {
                invoice.ConfirmPayment(transaction.Id, DateTime.UtcNow);
            }
            else
            {
                invoice.FailPayment(transaction.Id, result.Value.Message ?? "Payment finalization failed.");
            }

            await _invoiceRepository.UpdateAsync(invoice, ct);

            return result;
        }
    }
}
