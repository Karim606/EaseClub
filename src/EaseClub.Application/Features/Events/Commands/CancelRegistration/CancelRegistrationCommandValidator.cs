using FluentValidation;

namespace EaseClub.Application.Features.Events.Commands.CancelRegistration;

public class CancelRegistrationCommandValidator : AbstractValidator<CancelRegistrationCommand>
{
    public CancelRegistrationCommandValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty().WithMessage("Event ID is required.");

        RuleFor(x => x.RegistrationId)
            .NotEmpty().WithMessage("Registration ID is required.");
    }
}
