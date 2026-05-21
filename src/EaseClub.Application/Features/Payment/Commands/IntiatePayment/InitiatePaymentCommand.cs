using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Payment.Commands.IntiatePayment
{
    public record InitiatePaymentCommand(Guid InvoiceId, string Gateway) : IRequest<Result<IntiatePaymentResponse>>;

    public record IntiatePaymentResponse(Guid TransactionId,string SessionId);

    public record InitiatePaymentRequest(string Gateway);
}
