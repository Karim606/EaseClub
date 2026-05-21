using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Common.ValueObjects.Errors
{
    public static class MoneyErrors
    {
        public static Error MoneyAmountCantBeNonNegative = Error.Validation(code: "Money.Amount.Cant.Be.Non.Negative",
            description: "Money.Amount.Cant.Be.Non.Negative");

        public static Error CurrencyIsRequired = Error.Validation(code: "Curreny.Is.Required", description: "Curreny is required");

        public static Error ResultingMoneyCantBeNegative = Error.Validation(code: "Resulting.Money.Cant.Be.Negative",
            description: "Resulting Money Cant Be Negative.");

        public static Error FactorCantBeNegative = Error.Validation(code: "Factor.Cant.Be.Negative",
            description: "Factor.Cant.Be.Negative");

        public static Error CantOperateOnDifferentCurrrencies = Error.Validation(code: "Cant.Operate.On.Different.Currrencies",
            description: "CantOperateOnDifferentCurrrencies");
    }
}
