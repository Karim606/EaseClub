using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Notifications
{
    public interface IDeviceRepository
    {
        Task<List<DeviceDto>> GetDevicesByUserIdAsync(Guid userId, CancellationToken ct = default);
        Task UpsertDeviceAsync(DeviceDto device, CancellationToken ct = default);
        Task DeleteDeviceAsync(string fcmToken, CancellationToken ct = default);
        Task RemoveStaleTokensAsync(DateTime threshold, CancellationToken ct = default);
    }
}
