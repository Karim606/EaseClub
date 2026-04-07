using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Payment.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Payment.Commands.AttachOrderToTransaction
{
    public class AttachTransactionOrderIdCommandHandler(IPaymentTransactionRepository _transactionRepo,IUnitOfWork unitOfWork) : IRequestHandler<AttachOrderToTransactionCommand, Result<Success>>
    {

        public async Task<Result<Success>> Handle(AttachOrderToTransactionCommand request, CancellationToken ct)
        {
            var transaction = await _transactionRepo.GetByIdAsync(request.TransactionId);
            if (transaction is null)
                return Error.NotFound("Transaction not found");

            transaction.AttachExternalRef(request.OrderId);
            await unitOfWork.SaveChangesAsync(ct);

            return Result.Success;
        }
    }
}
