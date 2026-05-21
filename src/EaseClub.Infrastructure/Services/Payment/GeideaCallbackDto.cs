using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Services.Payment
{
    public record GeideaCallbackDto(
    string OrderId,
    decimal OrderAmount,
    string OrderCurrency,
    string PaymentMethod,
    string Status,
    string MerchantReferenceId,
    string TimeStamp,
    string Signature,
    string ResponseCode,
    string DetailedResponseCode,
    string DetailedResponseMessage
);
}
