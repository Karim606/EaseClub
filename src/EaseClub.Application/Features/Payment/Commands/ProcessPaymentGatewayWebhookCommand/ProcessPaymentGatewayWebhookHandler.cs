using Microsoft.Extensions.Logging;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Payment.Enums;
using EaseClub.Domain.Payment.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Payment.Commands.ProcessPaymentGatewayWebhookCommand
{
    public class ProcessPaymentGatewayWebhookCommandHandler(IInvoiceRepository invoiceRepository,
        IPaymentTransactionRepository transactionRepository,IUnitOfWork unitOfWork, IPaymentGateway paymentGateway,
        ILogger<ProcessPaymentGatewayWebhookCommandHandler> logger) : IRequestHandler<ProcessPaymentGatewayWebhookCommand, Result<Success>>
    {
        public async Task<Result<Success>> Handle(ProcessPaymentGatewayWebhookCommand request, CancellationToken cancellationToken)
        {

            var payload = paymentGateway.ValidateCallbackSignature(request.WebhookRequest);
            if (payload.IsError) { logger.LogError("Error in ProcessPaymentGatewayWebhookCommandHandler: {Error}", payload.TopError.ToLogObject()); return payload.TopError; }
            var result = await HandleTransactionAndInvoice(payload.Value);
            if (result.IsError) { logger.LogError("Error in ProcessPaymentGatewayWebhookCommandHandler: {Error}", result.TopError.ToLogObject()); return result.TopError; }
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success;
        }

        private async Task<Result<Success>> HandleTransactionAndInvoice(CallBackResultDto payload)
        {
            var transaction = await transactionRepository.GetByIdAsync(payload.TransactionId);
            if (transaction == null) { logger.LogError("NotFound error in ProcessPaymentGatewayWebhookCommandHandler: {Error}", Error.NotFound("Transaction not found").ToLogObject()); return Error.NotFound("Transaction not found"); }
            if (transaction.Amount != payload.Amount) { logger.LogError("Failure error in ProcessPaymentGatewayWebhookCommandHandler: {Error}", Error.Failure("Amount mismatch").ToLogObject()); return Error.Failure("Amount mismatch"); }
            if (transaction.Currency != payload.Currency) { logger.LogError("Failure error in ProcessPaymentGatewayWebhookCommandHandler: {Error}", Error.Failure("Currency mismatch").ToLogObject()); return Error.Failure("Currency mismatch"); }
            if (transaction.Status == PaymentTransactionStatus.Succeeded)
                return Result.Success;

            if (transaction.Status == PaymentTransactionStatus.Failed && payload.IsSuccess == false) 
                return Result.Success;

            if (string.IsNullOrEmpty(transaction.ExternalRef))
                transaction.AttachExternalRef(payload.ExternalRef);

            var invoice  = await invoiceRepository.GetInvoiceWithTransactions(transaction.InvoiceId);
            if (invoice == null) { logger.LogError("NotFound error in ProcessPaymentGatewayWebhookCommandHandler: {Error}", Error.NotFound("Invoice not found").ToLogObject()); return Error.NotFound("Invoice not found"); }
            var transactionId = transaction.Id;

            if (transaction.Status == PaymentTransactionStatus.Failed && payload.IsSuccess)
            {
                invoice.ForceMarkAsPaid(transactionId);
                return Result.Success;
            }
            else if (payload.IsSuccess)
            {

                invoice?.ConfirmPayment(transactionId, DateTime.UtcNow);
            }
            else
            {
                invoice.FailPayment(transactionId, payload.Message ?? "Payment failed");
            }
            return Result.Success;
        }
    }
}
