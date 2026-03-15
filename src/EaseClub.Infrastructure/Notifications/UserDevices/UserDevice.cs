using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Notifications.UserDevices
{
    public class UserDevice
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; } // Foreign key to Auth User
        public string DeviceId { get; set; } // From Flutter device_info_plus
        public string FcmToken { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
