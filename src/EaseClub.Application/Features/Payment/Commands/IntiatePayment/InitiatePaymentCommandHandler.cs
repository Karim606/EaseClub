//using Microsoft.Extensions.Logging;
//using EaseClub.Application.Common.Interfaces;
//using EaseClub.Domain.Common;
//using EaseClub.Domain.Common.Results;
//using EaseClub.Domain.Memberships;
//using EaseClub.Domain.Payment.Enums;
//using EaseClub.Domain.Payment.Repositories;
//using MediatR;

//namespace EaseClub.Application.Features.Payment.Commands.IntiatePayment
//{
//    public class InitiatePaymentCommandHandler(
//        IInvoiceRepository invoiceRepository, 
//        IPaymentGateway paymentGateway,
//        IEnrollmentRepository enrollmentRepository,
//        IUnitOfWork unitOfWork,
//        ILogger<InitiatePaymentCommandHandler> logger)
//        : IRequestHandler<InitiatePaymentCommand, Result<IntiatePaymentResponse>>
//    {
//        public async Task<Result<IntiatePaymentResponse>> Handle(InitiatePaymentCommand request, CancellationToken cancellationToken)
//        {

//            var invoice = await invoiceRepository.GetInvoiceWithTransactions(request.InvoiceId);
//            if (invoice is null) return Error.NotFound(description: "Invoice not found");


//            if (invoice.BillingItemType == BillingItemType.EnrollmentFirstInstallment)
//            {
//               var enrollment = await enrollmentRepository.GetByIdAsync(invoice.BillingItemId, cancellationToken);
//               if (enrollment == null) return Error.NotFound("Enrollment not found.");

//               if (enrollment.IsExpired(DateTime.UtcNow)) 
//               {
//                   return Error.Failure(description: "This invoice cannot be paid because the enrollment has expired. Please start a new enrollment.");
//               }
//            }

//            // Create Transaction (Domain)
//            var transactionResult = invoice.RecordAttempt(request.Gateway, null);
//            if (transactionResult.IsError) return transactionResult.TopError;
//            var transaction = transactionResult.Value;

//            // Call Payment Gateway to create session
//            var sessionResult = await paymentGateway.CreateSessionAsync(transaction, cancellationToken);

//            if (sessionResult.IsError) return sessionResult.TopError;
            
//            await unitOfWork.SaveChangesAsync(cancellationToken);

//            return new IntiatePaymentResponse(transaction.Id, sessionResult.Value.sessionId);
//        }
//    }
//}
