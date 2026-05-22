using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Features.Payment;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Payment.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Payment.Commands.FinalizeDirectPayment
{
    public record FinalizeDirectPaymentCommand(
        Guid TransactionId,
        CardDetailsDto CardDetails,
        string ThreeDSecureId
    ) : IRequest<Result<DirectPaymentResult>>;

    public class FinalizeDirectPaymentCommandHandler : IRequestHandler<FinalizeDirectPaymentCommand, Result<DirectPaymentResult>>
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IPaymentTransactionRepository _transactionRepository;
        private readonly IPaymentGateway _paymentGateway;
        private readonly IUnitOfWork _unitOfWork;
        ILogger<FinalizeDirectPaymentCommandHandler> _logger;
        public FinalizeDirectPaymentCommandHandler(
            IInvoiceRepository invoiceRepository,
            IPaymentTransactionRepository transactionRepository,
            IPaymentGateway paymentGateway,
            IUnitOfWork unitOfWork,
            ILogger<FinalizeDirectPaymentCommandHandler> logger)
        {
            _invoiceRepository = invoiceRepository;
            _transactionRepository = transactionRepository;
            _paymentGateway = paymentGateway;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<DirectPaymentResult>> Handle(FinalizeDirectPaymentCommand request, CancellationToken ct)
        {
            var transaction = await _transactionRepository.GetByIdAsync(request.TransactionId, ct);
            if (transaction == null)
                return Error.NotFound(description: "Transaction not found.");

            var invoice = await _invoiceRepository.GetInvoiceWithTransactions(transaction.InvoiceId, ct);
            if (invoice == null)
                return Error.NotFound(description: "Invoice associated with transaction not found.");

            var result = await _paymentGateway.FinalizePaymentAsync(transaction, request.CardDetails, request.ThreeDSecureId, ct);

            if (result.IsSuccess && result.Value.Status == "Success")
            {
                _logger.LogInformation("Payment finalized successfully for TransactionId: {TransactionId}", request.TransactionId);
                invoice.ConfirmPayment(transaction.Id, DateTime.UtcNow);
            }
            else
            {
                _logger.LogWarning("Payment finalization failed for TransactionId: {TransactionId}. Reason: {Reason}", request.TransactionId, result.Value.Message ?? "Payment finalization failed.");
                invoice.FailPayment(transaction.Id, result.Value.Message ?? "Payment finalization failed.");
            }

            await _unitOfWork.SaveChangesAsync(ct);

            return result;
        }
    }
}
