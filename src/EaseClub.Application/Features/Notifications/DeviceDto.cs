using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Notifications
{
    public record DeviceDto(Guid UserId, string DeviceId, string FcmToken, DateTime LastUpdated);
}
