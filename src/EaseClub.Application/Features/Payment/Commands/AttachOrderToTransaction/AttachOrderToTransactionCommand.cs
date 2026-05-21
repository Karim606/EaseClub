using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Payment.Commands.AttachOrderToTransaction
{
    public record AttachOrderToTransactionCommand(Guid TransactionId,string OrderId):IRequest<Result<Success>>;

    public record AttachOrderRequest(string OrderId);
}
