using EaseClub.Domain.Memberships;
using Microsoft.EntityFrameworkCore;

namespace EaseClub.Infrastructure.Data.Repositories
{
    public class EnrollmentRepository : EfRepository<Enrollment>, IEnrollmentRepository
    {
        public EnrollmentRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Enrollment> GetByIdWithDetailsAsync(Guid enrollmentId, CancellationToken ct = default)
        {
            return await _context.Enrollments
                .Include(e => e.MembershipPlan)
                .Include(e => e.Club)
                .FirstOrDefaultAsync(x => x.Id == enrollmentId, ct);
        }

        public async Task<Enrollment?> GetByFirstInvoiceIdAsync(Guid invoiceId, CancellationToken ct = default)
        {
            return await _context.Enrollments.FirstOrDefaultAsync(x => x.FirstInvoiceId == invoiceId, ct);
        }

        public async Task<Enrollment?> GetActiveByApplicationIdAsync(Guid applicationId, CancellationToken ct = default)
        {
            return await _context.Enrollments.FirstOrDefaultAsync(
                x => x.MembershipApplicationId == applicationId && 
                     x.Status == EnrollmentStatus.WaitingForFirstPayment &&
                     x.ExpiresAt > DateTime.UtcNow,
                ct);
        }

        public async Task<Enrollment?> GetActiveByMembershipIdAsync(Guid membershipId, CancellationToken ct = default)
        {
            return await _context.Enrollments.FirstOrDefaultAsync(
                x => x.ExistingMembershipId == membershipId && 
                     x.Status == EnrollmentStatus.WaitingForFirstPayment &&
                     x.ExpiresAt > DateTime.UtcNow,
                ct);
        }

        public async Task<Enrollment?> GetActiveDirectPayAsync(Guid userId, Guid planId, CancellationToken ct = default)
        {
            return await _context.Enrollments.FirstOrDefaultAsync(
                x => x.MemberId == userId && 
                     x.MembershipPlanId == planId && 
                     x.Source == EnrollmentSource.DirectPay &&
                     x.Status == EnrollmentStatus.WaitingForFirstPayment &&
                     x.ExpiresAt > DateTime.UtcNow,
                ct);
        }

        public async Task<List<Enrollment>> GetExpiredEnrollmentsAsync(DateTime now, CancellationToken ct = default)
        {
            return await _context.Enrollments
                .Where(x => x.Status == EnrollmentStatus.WaitingForFirstPayment && x.ExpiresAt <= now)
                .ToListAsync(ct);
        }

        public async Task<Enrollment?> GetLatestActiveByUserIdAsync(Guid userId, CancellationToken ct = default)
        {
            return await _context.Enrollments
                .Where(x => x.MemberId == userId && 
                           x.Status == EnrollmentStatus.WaitingForFirstPayment && 
                           x.ExpiresAt > DateTime.UtcNow)
                .Include(x => x.MembershipPlan)
                .Include(x => x.Club)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(ct);
        }
    }
}
