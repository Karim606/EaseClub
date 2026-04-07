using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Payment.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Payment
{
    public class PaymentTransaction : Entity
    {
        private PaymentTransaction() { }

        private PaymentTransaction(
            Guid id,
            Guid invoiceId,
            decimal amount,
            string gateway,
            PaymentMethod? method):base(id)
        {
            InvoiceId = invoiceId;
            Amount = amount;
            Method = method;
            Gateway = gateway;
            Status = PaymentTransactionStatus.Pending;
            CreatedAt = DateTime.UtcNow;
        }

        public Guid InvoiceId { get; private set; }
        public decimal Amount { get; private set; }
        public string? ExternalRef { get; private set; }
        public string Gateway { get; private set; }
        public PaymentMethod? Method { get; private set; }
        public PaymentTransactionStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? CompletedAt { get; private set; }
        public string? FailureReason { get; private set; }

        internal static PaymentTransaction Create(
            Guid id,
            Guid invoiceId,
            decimal amount,
            string gateway,
            PaymentMethod? method = null)
            => new(id,invoiceId, amount,gateway, method);

        internal void MarkAsSucceeded(DateTime completedAt)
        {

            Status = PaymentTransactionStatus.Succeeded;
            CompletedAt = completedAt;
        }

        internal void MarkAsFailed(string reason)
        {
            Status = PaymentTransactionStatus.Failed;
            FailureReason = reason;
        }

        public Result<Success> AttachExternalRef(string externalRef)
        {
            if (!string.IsNullOrEmpty(ExternalRef))
               return Error.Conflict("ExternalRef already set.");

            ExternalRef = externalRef;

            return Result.Success;
        }
    }
}
