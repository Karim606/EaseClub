using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Payment.Enums
{
    public enum PaymentTransactionStatus
    {
        Pending,    // gateway called, waiting
        Succeeded,  // gateway confirmed
        Failed      // gateway rejected or timeout
    }
}
