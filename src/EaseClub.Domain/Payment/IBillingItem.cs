using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Payment.Enums;
using System;

namespace EaseClub.Domain.Payment
{
    public interface IBillingItem
    {
        Guid Id { get; }
        decimal Amount { get; }
        string ReadableId { get; }

        public BillingItemType GetBillingType();

        /// <summary>
        /// Validates if the billing item is in a state that allows it to be paid.
        /// (e.g. Enrollment is not expired, Installment is not already paid or cancelled)
        /// </summary>
        Result<Success> CanBePaid();
    }
}
