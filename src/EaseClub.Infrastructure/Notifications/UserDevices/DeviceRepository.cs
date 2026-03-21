using EaseClub.Application.Features.Notifications;
using EaseClub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Notifications.UserDevices
{
    public class DeviceRepository : IDeviceRepository
    {
        private readonly AppDbContext _context;

        public DeviceRepository(AppDbContext context) => _context = context;

        public async Task<List<DeviceDto>> GetDevicesByUserIdAsync(Guid userId,CancellationToken ct = default)
        {
            return await _context.UserDevices
                .Where(d => d.UserId == userId)
                .Select(d => new DeviceDto(d.UserId,d.DeviceId, d.FcmToken, d.LastUpdated))
                .ToListAsync(ct);
        }

        public async Task UpsertDeviceAsync(DeviceDto device, CancellationToken ct = default)
        {
            var existing = await _context.UserDevices
                .FirstOrDefaultAsync(d => d.DeviceId == device.DeviceId);

            if (existing != null)
            {
                existing.FcmToken = device.FcmToken;
                existing.UserId = device.UserId;
                existing.LastUpdated = DateTime.UtcNow;
            }
            else
            {
               var deviceToAdd = new UserDevice { FcmToken = device.FcmToken, DeviceId = device.DeviceId, UserId = device.UserId, LastUpdated = DateTime.UtcNow };
                _context.UserDevices.Add(deviceToAdd);
            }
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteDeviceAsync(string fcmToken, CancellationToken ct = default)
        {
            var device = await _context.UserDevices.FirstOrDefaultAsync(d => d.FcmToken == fcmToken);
            if (device != null)
            {
                _context.UserDevices.Remove(device);
                await _context.SaveChangesAsync(ct);
            }
        }

        public async Task RemoveStaleTokensAsync(DateTime threshold, CancellationToken ct = default)
        {
            // EF Core 7+ approach (highly efficient, executes directly in DB)
            await _context.UserDevices
                .Where(d => d.LastUpdated < threshold)
                .ExecuteDeleteAsync(ct);
        }
    }
}
