using FluentValidation;

namespace EaseClub.Application.Features.Events.Commands.PublishEvent;

public class PublishEventCommandValidator : AbstractValidator<PublishEventCommand>
{
    public PublishEventCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Event ID is required.");
    }
}
