using EaseClub.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Payment.Errors
{
    public static class InvoiceErrors
    {
        // ========================
        // Validation Errors
        // ========================

        public static Error PayableIdRequired = Error.Validation(
            code: "Invoice.PayableIdRequired",
            description: "Payable ID is required.");

        public static Error InvalidAmount = Error.Validation(
            code: "Invoice.InvalidAmount",
            description: "Invoice amount must be greater than zero.");

        public static Error InvalidDueDate = Error.Validation(
            code: "Invoice.InvalidDueDate",
            description: "Due date must be in the future.");

        //public static Error InvoiceExpired = Error.Validation(
        //    code: "Invoice.Expired",
        //    description: "Invoice has expired and cannot be paid.");

        // ========================
        // Conflict Errors
        // ========================

        public static Error AlreadyPaid = Error.Conflict(
            code: "Invoice.AlreadyPaid",
            description: "Invoice is already paid.");

        public static Error PaymentAlreadyInProgress = Error.Conflict(
            code: "Invoice.PaymentAlreadyInProgress",
            description: "A payment attempt is already in progress.");

        public static Error CannotVoidPaidInvoice = Error.Conflict(
            code: "Invoice.CannotVoidPaid",
            description: "Cannot void a paid invoice.");

        public static Error InvoiceVoided = Error.Conflict(
            code: "Invoice.Voided",
            description: "Invoice has been voided and cannot be processed.");

        // ========================
        // Transaction Errors
        // ========================

        public static Error TransactionNotFound = Error.NotFound(
            code: "Invoice.TransactionNotFound",
            description: "Payment transaction not found.");

        public static Error TransactionNotPending = Error.Conflict(
            code: "Invoice.TransactionNotPending",
            description: "Transaction is not in a pending state.");

        public static Error TransactionAlreadySucceeded = Error.Conflict(
            code: "Invoice.TransactionAlreadySucceeded",
            description: "Transaction has already been completed successfully.");
    }
}
