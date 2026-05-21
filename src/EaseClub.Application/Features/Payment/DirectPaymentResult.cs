namespace EaseClub.Application.Features.Payment
{
    public record DirectPaymentResult(
        string Status,
        string? RedirectUrl = null,
        string? TransactionId = null,
        string? OrderId = null,
        string? Message = null
    );
}
