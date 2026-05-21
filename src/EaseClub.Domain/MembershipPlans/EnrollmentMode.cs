using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.MembershipPlans
{
    public enum EnrollmentMode
    {
        ApplicationForm,        // form → submit → admin review → pay → active
        DirectPay,              // no form → pay immediately → active
    }
}
