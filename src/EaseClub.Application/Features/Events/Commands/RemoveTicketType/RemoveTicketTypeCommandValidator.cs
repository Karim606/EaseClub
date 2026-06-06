using FluentValidation;

namespace EaseClub.Application.Features.Events.Commands.RemoveTicketType;

public class RemoveTicketTypeCommandValidator : AbstractValidator<RemoveTicketTypeCommand>
{
    public RemoveTicketTypeCommandValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty().WithMessage("Event ID is required.");

        RuleFor(x => x.TicketTypeId)
            .NotEmpty().WithMessage("Ticket Type ID is required.");
    }
}
