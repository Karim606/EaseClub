using EaseClub.Domain.Memberships;
using Microsoft.EntityFrameworkCore;

namespace EaseClub.Infrastructure.Data.Repositories
{
    public class PendingEnrollmentRepository : EfRepository<PendingEnrollment>, IPendingEnrollmentRepository
    {
        public PendingEnrollmentRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<PendingEnrollment?> GetByFirstInvoiceIdAsync(Guid invoiceId, CancellationToken ct = default)
        {
            return await _context.PendingEnrollments.FirstOrDefaultAsync(x => x.FirstInvoiceId == invoiceId, ct);
        }

        public async Task<PendingEnrollment?> GetActiveByApplicationIdAsync(Guid applicationId, CancellationToken ct = default)
        {
            return await _context.PendingEnrollments.FirstOrDefaultAsync(
                x => x.MembershipApplicationId == applicationId && x.Status == PendingEnrollmentStatus.WaitingForFirstPayment,
                ct);
        }
    }
}
