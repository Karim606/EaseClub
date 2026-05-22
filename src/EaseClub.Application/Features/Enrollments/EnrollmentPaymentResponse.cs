namespace EaseClub.Application.Features.Enrollments
{
    public record EnrollmentPaymentResponse(
        Guid EnrollmentId,
        Guid InvoiceId,
        decimal AmountToPay);
}
