using FluentValidation;

namespace EaseClub.Application.Features.Events.Commands.CancelEvent;

public class CancelEventCommandValidator : AbstractValidator<CancelEventCommand>
{
    public CancelEventCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Event ID is required.");
    }
}
