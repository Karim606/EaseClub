using EaseClub.Domain.Common.Interfaces;

namespace EaseClub.Domain.Memberships
{
    public interface IPendingEnrollmentRepository : IRepository<PendingEnrollment>
    {
        Task<PendingEnrollment?> GetByFirstInvoiceIdAsync(Guid invoiceId, CancellationToken ct = default);
        Task<PendingEnrollment?> GetActiveByApplicationIdAsync(Guid applicationId, CancellationToken ct = default);
    }
}
