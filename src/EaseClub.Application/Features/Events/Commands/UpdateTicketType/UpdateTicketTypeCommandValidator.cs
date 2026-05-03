using FluentValidation;

namespace EaseClub.Application.Features.Events.Commands.UpdateTicketType;

public class UpdateTicketTypeCommandValidator : AbstractValidator<UpdateTicketTypeCommand>
{
    public UpdateTicketTypeCommandValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty().WithMessage("Event ID is required.");

        RuleFor(x => x.TicketTypeId)
            .NotEmpty().WithMessage("Ticket Type ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ticket name is required.")
            .MaximumLength(100).WithMessage("Ticket name cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Ticket description cannot exceed 500 characters.");

        RuleFor(x => x.BasePrice)
            .GreaterThanOrEqualTo(0).WithMessage("Ticket price cannot be negative.");

        RuleFor(x => x.TotalQuantity)
            .GreaterThan(0).WithMessage("Ticket quantity must be greater than zero.");

        RuleFor(x => x.MaxPerMember)
            .GreaterThan(0).When(x => x.MaxPerMember.HasValue)
            .WithMessage("Max per member must be greater than zero.");
            
        RuleFor(x => x)
            .Must(x => !x.MaxPerMember.HasValue || x.MaxPerMember.Value <= x.TotalQuantity)
            .WithMessage("Max per member cannot be greater than the total ticket quantity.");
    }
}
