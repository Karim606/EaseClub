using FluentValidation;

namespace EaseClub.Application.Features.Events.Commands.RegisterForEvent;

public class RegisterForEventCommandValidator : AbstractValidator<RegisterForEventCommand>
{
    public RegisterForEventCommandValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty().WithMessage("Event ID is required.");

        RuleFor(x => x.RegistrantId)
            .NotEmpty().WithMessage("Registrant ID is required.");

        RuleFor(x => x.RegistrantName)
            .NotEmpty().WithMessage("Registrant name is required.");

        // Must have at least registrant attending OR some attendees
        RuleFor(x => x)
            .Must(x => x.IsRegistrantAttending || (x.Attendees != null && x.Attendees.Any()))
            .WithMessage("Either the registrant must attend or at least one attendee is required.");

        RuleForEach(x => x.Attendees)
            .ChildRules(attendee =>
            {
                attendee.RuleFor(a => a.TicketTypeId)
                    .NotEmpty().WithMessage("Ticket Type ID is required for each attendee.");

                attendee.RuleFor(a => a)
                    .Must(a => a.AttendeeId.HasValue || !string.IsNullOrWhiteSpace(a.AttendeeName))
                    .WithMessage("Each attendee must have either an ID or a name.");
            })
            .When(x => x.Attendees != null && x.Attendees.Any());
    }
}
