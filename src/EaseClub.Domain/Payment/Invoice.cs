using EaseClub.Domain.Clubs;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Member;
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
    public class Invoice : AuditableEntity,IBelongToMember
    {
        public Guid BillingItemId { get; private set; }
        // InstallmentId or RegistrationId
        public string BillingItemReadableId { get; private set; }
        public BillingItemType BillingItemType { get; private set; }
        // MembershipInstallment or EventRegistration

        public Guid ClubId { get; private set; }
        public Guid MemberId { get; private set; }
        public MemberUser Member { get; private set; }
        public Club Club { get; private set; }
        public decimal Amount { get; private set; }
        public InvoiceStatus Status { get; private set; }
        public string ReadableId { get; private set; }
        // Issued, Paid, Void

        private readonly List<PaymentTransaction> _Transactions = new();
        public IReadOnlyList<PaymentTransaction> Transactions
            => _Transactions.AsReadOnly();

        private Invoice(
            Guid id,
            Guid billingItemId,
            BillingItemType billingItemType,
            string billingItemReadableId,
            Guid clubId,
            Guid memberId,
            decimal amount):base(id)
        {
            BillingItemId = billingItemId;
            BillingItemType = billingItemType;
            BillingItemReadableId = billingItemReadableId;
            ClubId = clubId;
            MemberId = memberId;
            Amount = amount;
            Status = InvoiceStatus.Issued;
        }

        public static Result<Invoice> Create(
            IBillingItem billingItem,
            Guid clubId,
            Guid memberId,
            decimal amount)
        {
            if (billingItem.Id == Guid.Empty)
                return InvoiceErrors.PayableIdRequired;
            if (amount <= 0)
                return InvoiceErrors.InvalidAmount;

            var invoice =  new Invoice(
                Guid.NewGuid(),
                billingItem.Id,
                billingItem.GetBillingType(),
                billingItem.ReadableId,
                clubId,
                memberId,
                amount);

            invoice.ReadableId = InvoiceIdGenerator.Generate();

            return invoice;

        }

        public Result<PaymentTransaction> RecordAttempt(
            string GatewayName,
            string? method = null)
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
            var transation = PaymentTransaction.Create(Guid.NewGuid(), Id, Amount, GatewayName, method);
            _Transactions.Add(transation);

            return transation;
        }

        public Result<Success> ConfirmPayment(
            Guid transactionId,
            DateTime paidAt)
        {
            if (Status == InvoiceStatus.Void)
                return InvoiceErrors.InvoiceVoided;

            var transaction = _Transactions
                .FirstOrDefault(t => t.Id == transactionId);

            if (transaction == null)
                return InvoiceErrors.TransactionNotFound;

            if (transaction.Status == PaymentTransactionStatus.Succeeded)
                return Result.Success;

            if (transaction.Status != PaymentTransactionStatus.Pending)
                return InvoiceErrors.TransactionNotPending;

            transaction.MarkAsSucceeded(paidAt);
            Status = InvoiceStatus.Paid;

            RaiseDomainEvent(new InvoicePaidEvent(
                Id,BillingItemId,BillingItemType, Amount, MemberId, ClubId));

            return Result.Success;
        }

        public Result<Success> FailPayment(
            Guid transactionId,
            string reason)
        {
            var transaction = _Transactions
                .FirstOrDefault(t => t.Id == transactionId);

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

        public Result<Success> ForceMarkAsPaid(Guid transactionId)
        {
            if (Status == InvoiceStatus.Void)
                return InvoiceErrors.InvoiceVoided;

            if (Status == InvoiceStatus.Paid)
                return Result.Success; // idempotent

            var transaction = _Transactions
                .FirstOrDefault(t => t.Id == transactionId);

            if (transaction == null)
                return InvoiceErrors.TransactionNotFound;

            // 1. Mark this transaction as succeeded (forcefully)
            if (transaction.Status != PaymentTransactionStatus.Succeeded)
            {
                transaction.MarkAsSucceeded(DateTime.UtcNow);
            }

            // 2. Cancel all other pending transactions
            foreach (var t in _Transactions.Where(t =>
                t.Id != transaction.Id &&
                t.Status == PaymentTransactionStatus.Pending))
            {
                t.MarkAsFailed("Superseded by successful payment");
            }

            // 3. Update invoice
            Status = InvoiceStatus.Paid;

            // 4. Raise domain event (ONLY ONCE)
            RaiseDomainEvent(new InvoicePaidEvent(
                Id, BillingItemId, BillingItemType, Amount, MemberId, ClubId));

            return Result.Success;
        }

        public Result<Success> Reconcile(
        Guid newBillingItemId,
        BillingItemType newType)
        {
            if (BillingItemType != BillingItemType.PendingEnrollmentFirstInstallment)
                return Error.Conflict("Invoice.CannotReconcile",
                    "Only pending enrollment invoices can be reconciled.");

            if (Status != InvoiceStatus.Paid)
                return Error.Conflict("Invoice.MustBePaidToReconcile",
                    "Invoice must be paid before reconciling.");

            BillingItemId = newBillingItemId;
            BillingItemType = newType;
            return Result.Success;
        }
    }
}
