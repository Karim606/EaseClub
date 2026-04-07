using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Payment.Enums;
using EaseClub.Domain.Payment.Errors;
using EaseClub.Domain.Payment.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace EaseClub.Domain.Payment
{
    public class Invoice : AuditableEntity
    {
        public Guid PayableId { get; private set; }
        // InstallmentId or RegistrationId

        public PayableType PayableType { get; private set; }
        // MembershipInstallment or EventRegistration

        public Guid ClubId { get; private set; }
        public Guid UserId { get; private set; }
        public decimal Amount { get; private set; }
        public DateTime DueDate { get; private set; }
        public InvoiceStatus Status { get; private set; }
        // Issued, Paid, Void

        private readonly List<PaymentTransaction> _Transactions = new();
        public IReadOnlyList<PaymentTransaction> Transactions
            => _Transactions.AsReadOnly();

        private Invoice(
            Guid id,
            Guid payableId,
            PayableType payableType,
            Guid clubId,
            Guid userId,
            decimal amount,
            DateTime dueDate):base(id)
        {
            PayableId = payableId;
            PayableType = payableType;
            ClubId = clubId;
            UserId = userId;
            Amount = amount;
            DueDate = dueDate;
            Status = InvoiceStatus.Issued;
        }

        public static Result<Invoice> Create(
            Guid payableId,
            PayableType payableType,
            Guid clubId,
            Guid userId,
            decimal amount,
            DateTime dueDate)
        {
            if (payableId == Guid.Empty)
                return InvoiceErrors.PayableIdRequired;
            if (amount <= 0)
                return InvoiceErrors.InvalidAmount;
            if (dueDate.Date < DateTime.UtcNow.Date)
                return InvoiceErrors.InvalidDueDate;

            return new Invoice(
                Guid.NewGuid(),
                payableId, payableType,
                clubId, userId,
                amount, dueDate);
        }

        public Result<Success> RecordAttempt(
            string GatewayName,
            string externalRef,
            PaymentMethod? method = null)
        {
            if (Status == InvoiceStatus.Paid)
                return InvoiceErrors.AlreadyPaid;

            if (Status == InvoiceStatus.Void)
                return InvoiceErrors.InvoiceVoided;

            // One pending attempt at a time
            if (_Transactions.Any(t => t.Status == PaymentTransactionStatus.Pending))
            {
                foreach (var t in _Transactions.Where(t => t.Status == PaymentTransactionStatus.Pending))
                {
                    t.MarkAsFailed("New payment attempt initiated");
                }
            }

            _Transactions.Add(
                PaymentTransaction.Create(Guid.NewGuid(),Id, Amount, externalRef, method));

            return Result.Success;
        }

        public Result<Success> ConfirmPayment(
            string externalRef,
            DateTime paidAt)
        {
            if (Status == InvoiceStatus.Void)
                return InvoiceErrors.InvoiceVoided;

            var transaction = _Transactions
                .FirstOrDefault(t => t.ExternalRef == externalRef);

            if (transaction == null)
                return InvoiceErrors.TransactionNotFound;

            if (transaction.Status == PaymentTransactionStatus.Succeeded)
                return Result.Success;

            if (transaction.Status != PaymentTransactionStatus.Pending)
                return InvoiceErrors.TransactionNotPending;

            transaction.MarkAsSucceeded(paidAt);
            Status = InvoiceStatus.Paid;

            RaiseDomainEvent(new InvoicePaidEvent(
                Id, PayableId, PayableType, Amount, UserId, ClubId));

            return Result.Success;
        }

        public Result<Success> FailPayment(
            string externalRef,
            string reason)
        {
            var transaction = _Transactions
                .FirstOrDefault(t => t.ExternalRef == externalRef);

            if (transaction == null)
                return InvoiceErrors.TransactionNotFound;

            if (transaction.Status == PaymentTransactionStatus.Failed)
                return Result.Success; // idempotent

            if (transaction.Status == PaymentTransactionStatus.Succeeded)
                return InvoiceErrors.TransactionAlreadySucceeded;

            transaction.MarkAsFailed(reason);
            // Invoice stays Issued — user can retry

            return Result.Success;
        }

        public Result<Success> Void()
        {
            if (Status == InvoiceStatus.Paid)
                return InvoiceErrors.CannotVoidPaidInvoice;

            Status = InvoiceStatus.Void;
            return Result.Success;
        }
    }
}
