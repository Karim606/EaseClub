using EaseClub.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using EaseClub.Application.Features.Notifications;
using EaseClub.Infrastructure.Notifications.UserDevices;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Api.IntegrationTests.Common
{
    public class ApiWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // 1. SET ENVIRONMENT HERE (The earliest possible place)
            builder.UseEnvironment("testing");

            builder.ConfigureServices( services =>
            {
                services.Replace(ServiceDescriptor.Scoped<IDeviceRepository, IntegrationTestDeviceRepository>());
            });


        }
    }

    public class IntegrationTestDeviceRepository : IDeviceRepository
    {
        private readonly DeviceRepository _inner;
        private readonly AppDbContext _context;

        public IntegrationTestDeviceRepository(AppDbContext context)
        {
            _inner = new DeviceRepository(context);
            _context = context;
        }

        public Task<List<DeviceDto>> GetDevicesByUserIdAsync(Guid userId, CancellationToken ct = default)
            => _inner.GetDevicesByUserIdAsync(userId, ct);

        public Task UpsertDeviceAsync(DeviceDto device, CancellationToken ct = default)
            => _inner.UpsertDeviceAsync(device, ct);

        public Task DeleteDeviceAsync(string fcmToken, CancellationToken ct = default)
            => _inner.DeleteDeviceAsync(fcmToken, ct);

        public async Task RemoveStaleTokensAsync(DateTime threshold, CancellationToken ct = default)
        {
            var stale = await _context.UserDevices
                .Where(d => d.LastUpdated < threshold)
                .ToListAsync(ct);
            _context.UserDevices.RemoveRange(stale);
            await _context.SaveChangesAsync(ct);
        }
    }
}
