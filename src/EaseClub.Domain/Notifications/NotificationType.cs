using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Notifications
{
    public enum NotificationType
    {
        General = 0,

        // Membership
        MembershipApplicationSubmitted = 1,
        MembershipApplicationApproved = 2,
        MembershipApplicationRejected = 3,
        MembershipActivated = 4,
        MembershipExpiringSoon = 5,

        // Booking / Events
        EventCreated = 10,
        EventUpdated = 11,
        EventCancelled = 12,
        BookingConfirmed = 13,
        BookingCancelled = 14,

        // Payments
        PaymentSuccessful = 20,
        PaymentFailed = 21,
        InvoiceGenerated = 22,

        // System
        AdminAlert = 30,
        SystemAnnouncement = 31
    }

}
