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
using Microsoft.Extensions.Logging;
using EaseClub.Domain.Memberships;
using EaseClub.Domain.Payment.Enums;
using EaseClub.Domain.Events;
using EaseClub.Domain.Events.Entities;

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
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IMembershipRepository _membershipRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProcessDirectPaymentCommandHandler> _logger;

        public ProcessDirectPaymentCommandHandler(
            IInvoiceRepository invoiceRepository,
            IPaymentTransactionRepository transactionRepository,
            IPaymentGateway paymentGateway,
            IEnrollmentRepository enrollmentRepository,
            IMembershipRepository membershipRepository,
            IEventRepository eventRepository,
            IUnitOfWork unitOfWork,
            ILogger<ProcessDirectPaymentCommandHandler> logger)
        {
            _invoiceRepository = invoiceRepository;
            _transactionRepository = transactionRepository;
            _paymentGateway = paymentGateway;
            _enrollmentRepository = enrollmentRepository;
            _membershipRepository = membershipRepository;
            _eventRepository = eventRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<DirectPaymentResult>> Handle(ProcessDirectPaymentCommand request, CancellationToken ct)
        {
            var invoice = await _invoiceRepository.GetInvoiceWithTransactions(request.InvoiceId, ct);
            if (invoice == null)
                return Error.NotFound(description: "Invoice not found.");

            // 1. Fetch the Billing Item generically to check for expiry/validity
            IBillingItem? billingItem = null;
            if (invoice.BillingItemType == BillingItemType.EnrollmentFirstInstallment)
            {
                billingItem = await _enrollmentRepository.GetByIdAsync(invoice.BillingItemId, ct);
            }
            else if (invoice.BillingItemType == BillingItemType.MembershipInstallment)
            {
                billingItem = await _membershipRepository.GetInstallmentByIdAsync(invoice.BillingItemId, ct);
            }
            else if (invoice.BillingItemType == BillingItemType.EventRegistration)
            {
                billingItem = await _eventRepository.GetRegistrationByIdAsync(invoice.BillingItemId, ct);
            }

            if (billingItem == null)
                return Error.NotFound(description: "Underlying billing item not found.");

            // 2. Domain-level validation (e.g. Enrollment expiry)
            var canBePaidResult = invoice.CanBePaid(billingItem);
            if (canBePaidResult.IsError)
            {
                _logger.LogWarning("Payment blocked for Invoice {InvoiceId}: {Reason}", invoice.Id, canBePaidResult.TopError.Description);
                return canBePaidResult.TopError;
            }

            // 3. Record Attempt in the Invoice Domain
            var recordResult = invoice.RecordAttempt("Geidea", "Card");
            if (!recordResult.IsSuccess)
            {
                _logger.LogWarning("Failed to record payment attempt for InvoiceId: {InvoiceId}. Reason: {Reason}", request.InvoiceId, recordResult.TopError.Description);
                return recordResult.TopError;
            }

            var transaction = recordResult.Value;
            await _transactionRepository.AddAsync(transaction, ct);

            // 4. Process payment via gateway
            var result = await _paymentGateway.ProcessPaymentAsync(transaction, request.CardDetails, ct);

            if (result.IsSuccess)
            {
                if (result.Value.Status == "Success")
                {
                    _logger.LogInformation("Payment successful for InvoiceId: {InvoiceId}. TransactionId: {TransactionId}.", request.InvoiceId, transaction.Id);
                    invoice.ConfirmPayment(transaction.Id, DateTime.UtcNow);
                    transaction.AttachExternalRef(result.Value.OrderId ?? "");
                }
                else if (result.Value.Status == "RequiresAction")
                {
                    _logger.LogInformation("Payment requires additional action for InvoiceId: {InvoiceId}.", request.InvoiceId);
                    transaction.AttachExternalRef(result.Value.OrderId ?? "");
                }
            }
            else
            {
                _logger.LogError("Payment failed for InvoiceId: {InvoiceId}. TransactionId: {TransactionId}. Reason: {Reason}", request.InvoiceId, transaction.Id, result.TopError.Description);
                invoice.FailPayment(transaction.Id, result.TopError.Description);
            }

            await _invoiceRepository.UpdateAsync(invoice, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return result;
        }
    }
}
