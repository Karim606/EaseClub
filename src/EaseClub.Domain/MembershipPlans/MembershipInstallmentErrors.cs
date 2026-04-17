using EaseClub.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.MembershipPlans
{
    public static  class MembershipInstallmentErrors
    {
        public static Error MembershipCycleIdMustBeProvided = 
            Error.Validation(code: "MembershipInstallment.MembershipCycleId.Must.Be.Provided",
                description: "Membership cycle ID must be provided for a membership installment.");

        public static Error CantMarkPaidPlanAsOverdue = 
            Error.Validation(code: "MembershipInstallment.Cant.Mark.Paid.Plan.As.Overdue",
                description: "Cannot mark a paid installment as overdue.");

        public static Error NotOverDuedYet = 
            Error.Validation(code: "MembershipInstallment.Not.OverDued.Yet",
                description: "Installment is not past due date yet, cannot mark as overdue.");
        public static Error CantMarkUnPendingInstallmentAsOverDue = 
            Error.Validation(code: "MembershipInstallment.Cant.Mark.UnPending.Installment.As.Overdue",
                description: "Only pending installments can be marked as overdue.");

        public static Error InvoiceIdMustBeProvidedWhenMarkingAsPaid = 
            Error.Validation(code: "MembershipInstallment.InvoiceId.Must.Be.Provided.When.Marking.As.Paid",
                description: "Invoice ID must be provided when marking an installment as paid.");

        public static Error InstallmentAmountMustBeGreaterThanZero = 
            Error.Validation(code: "MembershipInstallment.Amount.Must.Be.GreaterThanZero",
                description: "Installment amount must be greater than zero.");

        public static Error InstallmentDueDateMustBeInTheFuture = 
            Error.Validation(code: "MembershipInstallment.DueDate.Must.Be.InTheFuture",
                description: "Installment due date must be in the future.");

        public static Error InstallmentOrderMustBeNonNegative = 
            Error.Validation(code: "MembershipInstallment.Order.Must.Be.NonNegative",
                description: "Installment order must be non-negative.");

        public static Error InstallmentAlreadyPaid = 
            Error.Validation(code: "MembershipInstallment.Already.Paid",
                description: "This installment has already been marked as paid.");

        public static Error OnlyPendingInstallmentOrOverDueCanBePaid = 
            Error.Validation(code: "MembershipInstallment.Only.Pending.Or.OverDued.Can.Be.Paid",
                description: "Only pending or overdue installments can be marked as paid.");
    }
}
