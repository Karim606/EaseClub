using EaseClub.Domain.Payment;
using EaseClub.Domain.Common.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EaseClub.Application.Features.Payment.Commands.ProcessPaymentGatewayWebhookCommand;

namespace EaseClub.Application.Features.Payment
{
    public interface IPaymentGateway
    {
        Task<Result<PaymentSessionResult>> CreateSessionAsync(PaymentTransaction transaction, CancellationToken ct);

        Task<Result<DirectPaymentResult>> ProcessPaymentAsync(PaymentTransaction transaction, CardDetailsDto cardDetails, CancellationToken ct);

        Task<Result<DirectPaymentResult>> FinalizePaymentAsync(PaymentTransaction transaction, string threeDSecureId, CancellationToken ct);

        Result<CallBackResultDto> ValidateCallbackSignature(object payload);
    }
}
