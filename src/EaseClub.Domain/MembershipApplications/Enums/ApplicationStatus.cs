using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.MembershipApplications.Enums
{
   public enum ApplicationStatus {

        Draft,
        Submitted,     // Pending Review
        //UnderReview,   // Admin is looking at it
        //NeedsChanges,  // Admin sent it back to the user
        Approved,      // The decision is made!
        Rejected       // The decision is made!
    }

}
