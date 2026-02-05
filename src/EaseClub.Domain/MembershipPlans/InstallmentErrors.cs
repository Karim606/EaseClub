using EaseClub.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.MembershipPlans
{
    public static class InstallmentErrors
    {
        public static Error InstallmentPercentageMustBeGreaterThanZeroAndLessThanOrEqual100 =
            Error.Validation(code: "Installment.Percentage.Must.Be.GreaterThanZero.And.LessThan.Or.Equal100",
                description: "Installment percentage must be greater than 0 and less or equal than 100.");

        public static Error InstallmentDueAfterDaysMustBeNonNegative =
            Error.Validation(code: "Installment.DueAfterDays.Must.Be.NonNegative",
                description: "Installment due after days must be non-negative.");

        public static Error InstallmentOrderIndexMustBeNonNegative = Error.Validation(code:"Installment.OrderIndex.Must.Be.NonNegative",
                description: "Installment order index must be non-negative.");
    }
}
