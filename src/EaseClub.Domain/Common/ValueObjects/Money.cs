using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Common.ValueObjects.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Common.ValueObjects
{
    public record Money
    {
        public decimal Amount { get; }
        public string Currency { get; }

        private Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency.ToUpperInvariant();
        }

        // Clean naming: "Money.Of" or "Money.From"
        public static Result<Money> Of(decimal amount, string currency = "EGP")
        {
            if (amount < 0) return MoneyErrors.MoneyAmountCantBeNonNegative;
            if (string.IsNullOrWhiteSpace(currency)) return MoneyErrors.CurrencyIsRequired;

            return new Money(amount, currency.ToUpperInvariant());
        }

        public static Money Zero(string currency = "EGP") => new(0, currency);

        public Result<Money> Add(Money other)
        {
            if (Currency != other.Currency)
                return MoneyErrors.CantOperateOnDifferentCurrrencies;

            return new Money(Amount + other.Amount, Currency);
        }

        public Result<Money> Subtract(Money other)
        {
            if (Currency != other.Currency)
                return MoneyErrors.CantOperateOnDifferentCurrrencies;

            if (Amount - other.Amount < 0)
                return MoneyErrors.MoneyAmountCantBeNonNegative;

            return new Money(Amount - other.Amount, Currency);
        }
    }
}
