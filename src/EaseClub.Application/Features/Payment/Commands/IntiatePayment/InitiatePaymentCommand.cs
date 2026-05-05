//using EaseClub.Application.Common;
//using EaseClub.Application.Common.Interfaces;
//using EaseClub.Domain.Common.Results;
//using EaseClub.Domain.Payment;
//using MediatR;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace EaseClub.Application.Features.Payment.Commands.IntiatePayment
//{
//    public record InitiatePaymentCommand(Guid InvoiceId, string Gateway) : IRequest<Result<IntiatePaymentResponse>>, IRequireResourceValidation
//    {
//        public IEnumerable<OwnershipRule> Rules()
//        {
//            yield return new OwnershipRule(
//                async (auth, _) => await auth.DoesResourceBelongToCurrentUserAsync<Invoice>(InvoiceId),
//                nameof(InvoiceId),
//                InvoiceId);
//        }
//    }

//    public record IntiatePaymentResponse(Guid TransactionId,string SessionId);

//    public record InitiatePaymentRequest(string Gateway);
//}
