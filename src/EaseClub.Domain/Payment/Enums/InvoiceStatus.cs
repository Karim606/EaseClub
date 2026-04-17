using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Payment.Enums
{
    public enum InvoiceStatus
    {
        Issued,     // waiting for payment
        Paid,       // confirmed paid
        Void        // cancelled — no payment expected
    }
}
