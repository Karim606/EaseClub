using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common.Interfaces;
using EaseClub.Domain.Memberships;
using EaseClub.Domain.Payment.Enums;
using EaseClub.Domain.Payment.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EaseClub.Infrastructure.Services.BackgroundJobs
{
    public class EnrollmentCleanupWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EnrollmentCleanupWorker> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(6);

        public EnrollmentCleanupWorker(IServiceProvider serviceProvider, ILogger<EnrollmentCleanupWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Enrollment Cleanup Worker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupExpiredEnrollmentsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while cleaning up expired enrollments.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        private async Task CleanupExpiredEnrollmentsAsync(CancellationToken ct)
        {
            using var scope = _serviceProvider.CreateScope();
            var enrollmentRepo = scope.ServiceProvider.GetRequiredService<IEnrollmentRepository>();
            var invoiceRepo = scope.ServiceProvider.GetRequiredService<IInvoiceRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var expiredOnes = await enrollmentRepo.GetExpiredEnrollmentsAsync(DateTime.UtcNow, ct);

            if (!expiredOnes.Any()) return;

            _logger.LogInformation("Found {Count} expired enrollments to clean up.", expiredOnes.Count);

            foreach (var enrollment in expiredOnes)
            {
                enrollment.MarkExpired();

                // Also void the associated invoice if it exists and is unpaid (Issued)
                if (enrollment.FirstInvoiceId.HasValue)
                {
                    var invoice = await invoiceRepo.GetByIdAsync(enrollment.FirstInvoiceId.Value, ct);
                    if (invoice != null && invoice.Status == InvoiceStatus.Issued)
                    {
                        invoice.Void();
                    }
                }
            }

            await unitOfWork.SaveChangesAsync(ct);
            _logger.LogInformation("Cleanup of expired enrollments completed.");
        }
    }
}
