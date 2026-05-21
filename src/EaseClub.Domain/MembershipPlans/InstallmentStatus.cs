using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.MembershipPlans
{
    public enum InstallmentStatus
    {
        Pending = 1,  // Standard pending state
        Paid = 2,        // Success
        Overdue = 3,     // Grace period/deadline passed
        Cancelled = 4,   // Membership terminated, payment no longer required
      
    }
}
