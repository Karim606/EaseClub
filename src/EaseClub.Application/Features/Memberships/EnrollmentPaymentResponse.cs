namespace EaseClub.Application.Features.Memberships
{
    public record EnrollmentPaymentResponse(
        Guid PendingEnrollmentId,
        Guid InvoiceId);
}
