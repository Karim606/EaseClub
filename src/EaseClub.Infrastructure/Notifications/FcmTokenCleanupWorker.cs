using EaseClub.Application.Features.Notifications;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Notifications
{
    public class FcmTokenCleanupWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24);

        public FcmTokenCleanupWorker(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var repo = scope.ServiceProvider.GetRequiredService<IDeviceRepository>();

                    // Define "Stale" as 30 days old
                    var threshold = DateTime.UtcNow.AddDays(-30);

                    await repo.RemoveStaleTokensAsync(threshold);
                }

                // Wait 24 hours before running again
                await Task.Delay(_checkInterval, stoppingToken);
            }
        }
    }
}
