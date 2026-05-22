using EaseClub.Domain.Common.Interfaces;

namespace EaseClub.Domain.Memberships
{
    public interface IEnrollmentRepository : IRepository<Enrollment>
    {
        Task<Enrollment> GetByIdWithDetailsAsync(Guid enrollmentId, CancellationToken ct = default);
        Task<Enrollment?> GetByFirstInvoiceIdAsync(Guid invoiceId, CancellationToken ct = default);
        Task<Enrollment?> GetActiveByApplicationIdAsync(Guid applicationId, CancellationToken ct = default);
        Task<Enrollment?> GetActiveByMembershipIdAsync(Guid membershipId, CancellationToken ct = default);
        Task<Enrollment?> GetActiveDirectPayAsync(Guid userId, Guid planId, CancellationToken ct = default);
        Task<List<Enrollment>> GetExpiredEnrollmentsAsync(DateTime now, CancellationToken ct = default);
        Task<Enrollment?> GetLatestActiveByUserIdAsync(Guid userId, CancellationToken ct = default);
        Task<List<Enrollment>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct = default);
    }
}
