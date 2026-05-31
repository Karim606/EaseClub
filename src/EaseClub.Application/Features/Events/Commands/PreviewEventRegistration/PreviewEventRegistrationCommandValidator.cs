using FluentValidation;

namespace EaseClub.Application.Features.Events.Commands.PreviewEventRegistration;

public class PreviewEventRegistrationCommandValidator : AbstractValidator<PreviewEventRegistrationCommand>
{
    public PreviewEventRegistrationCommandValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty().WithMessage("Event ID is required.");

        RuleFor(x => x.RegistrantId)
            .NotEmpty().WithMessage("Registrant ID is required.");

        RuleFor(x => x.Attendees)
            .NotEmpty()
            .When(x => !x.IsRegistrantAttending)
            .WithMessage("At least one attendee is required if the registrant is not attending.");

        RuleForEach(x => x.Attendees).ChildRules(attendee =>
        {
            attendee.RuleFor(a => a.TicketTypeId)
                .NotEmpty().WithMessage("Ticket Type ID is required for each attendee.");

            attendee.RuleFor(a => a)
                .Must(a => a.AttendeeId.HasValue || !string.IsNullOrWhiteSpace(a.AttendeeName))
                .WithMessage("Each attendee must have either an ID or a specific name.");
        });
    }
}
