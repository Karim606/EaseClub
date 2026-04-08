using EaseClub.Domain.Payment.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Payment
{
  public  interface IBillingItem
    {
        Guid Id { get; }
        decimal Amount { get; }
        string ReadableId { get; }

        public BillingItemType GetBillingType();
    }
}
