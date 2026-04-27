namespace EaseClub.Application.Features.Payment
{
    public record CardDetailsDto(
        string Number,
        string ExpiryMonth,
        string ExpiryYear,
        string Cvv,
        string HolderName
    );
}
