using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Payment;
using EaseClub.Domain.Payment.Enums;
using EaseClub.Domain.Payment.Errors;
using EaseClub.Domain.Payment.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Payment.Commands.IntiatePayment
{
    public class InitiatePaymentCommandHandler(IInvoiceRepository invoiceRepository, IPaymentGateway paymentGateway,IUnitOfWork unitOfWork)
        : IRequestHandler<InitiatePaymentCommand, Result<IntiatePaymentResponse>>
    {

        public async Task<Result<IntiatePaymentResponse>> Handle(InitiatePaymentCommand request, CancellationToken cancellationToken)
        {
            var invoice = await invoiceRepository.GetInvoiceWithTransactions(request.InvoiceId);
            if (invoice is null)
                return Error.NotFound(description: "Invoice not found");

            // Create Transaction (Domain)
            var transactionResult = invoice.RecordAttempt(request.Gateway, null);
            if (transactionResult.IsError)
                return transactionResult.TopError;

            var transaction = transactionResult.Value;

            // Call Payment Gateway to create session
            var sessionId = await paymentGateway.CreateSessionAsync(transaction,cancellationToken);

            if(sessionId.IsError) return sessionId.TopError;

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return new IntiatePaymentResponse(transaction.Id, sessionId.Value.sessionId);

        }
    }
}
