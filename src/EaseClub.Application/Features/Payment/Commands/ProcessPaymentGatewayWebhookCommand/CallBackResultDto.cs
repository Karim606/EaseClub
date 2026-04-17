using EaseClub.Domain.Payment.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Payment.Commands.ProcessPaymentGatewayWebhookCommand
{
    public class CallBackResultDto
    {
        public CallBackResultDto(Guid transactionId, string paymentMethod, string externalRef, bool isSuccess, string? message)
        {
            TransactionId = transactionId;
            PaymentMethod = paymentMethod;
            ExternalRef = externalRef;
            IsSuccess = isSuccess;
            Message = message;
        }

        public Guid TransactionId { get; set; }
        public string PaymentMethod { get; set; }
        public string ExternalRef { get; set; } = null!;
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = null!;

    }
}
