using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;

namespace EaseClub.Domain.Events;

public static class EventErrors
{
    // Event Errors
    public static Error InvalidClub => Error.Validation("Event.InvalidClub", "Club ID is required.");
    public static Error PastDate => Error.Validation("Event.PastDate", "Event start date must be in the future.");
    public static Error InvalidEndDate => Error.Validation("Event.InvalidEndDate", "End date must be after start date.");
    public static Error EndDateBeforeStartDate => Error.Validation("Event.EndDateBeforeStartDate", "Event end date cannot be before or equal to start date.");
    public static Error InvalidCapacity => Error.Validation("Event.InvalidCapacity", "Event capacity must be greater than zero.");
    public static Error CapacityLessThanSold => Error.Validation("Event.CapacityLessThanSold", "New capacity cannot be less than the number of tickets already sold.");
    public static Error NotDraft(string action) => Error.Validation("Event.NotDraft", $"Can only {action} draft events.");
    public static Error CapacityTooSmall => Error.Validation("Event.CapacityTooSmall", "New capacity cannot be less than the sum of existing ticket quantities.");
    public static Error InvalidTicketCategory(string category, string audience) => Error.Validation("Event.InvalidTicketCategory", $"Category {category} is not allowed for audience {audience}.");
    public static Error CapacityExceeded => Error.Validation("Event.CapacityExceeded", "Adding or updating this ticket type would exceed the total event capacity.");
    public static Error DuplicateTicketCategory(string category) => Error.Validation("Event.DuplicateTicketCategory", $"A ticket with category {category} already exists for this event.");
    public static Error TicketNotFound => Error.NotFound("Event.TicketNotFound", "Ticket type not found.");
    public static Error TicketNotFoundForCategory(string category) => Error.NotFound("Event.TicketNotFoundForCategory", $"No ticket type configured for category '{category}'. Admin must add this ticket type.");
    public static Error IncompatibleAudience(string newAudience, string ticketName, string category) => Error.Validation("Event.IncompatibleAudience", $"Cannot change audience to {newAudience} because existing ticket '{ticketName}' has category '{category}' which is not allowed.");
    
    // Registration Errors (Event Level)
    public static Error NotPublished => Error.Validation("Event.NotPublished", "Cannot register for an event that is not published.");
    public static Error EventAlreadyStarted => Error.Validation("Event.EventAlreadyStarted", "Cannot register for an event that has already started.");
    public static Error NoAttendees => Error.Validation("Event.NoAttendees", "At least one attendee is required to register.");
    public static Error RegistrantMustBeMember => Error.Validation("Event.RegistrantMustBeMember", "This event requires the registrant to be a club member.");
    public static Error MemberTicketRequired => Error.Validation("Event.MemberTicketRequired", "One or more selected tickets require an active membership.");
    public static Error AttendeeIdRequired => Error.Validation("Event.AttendeeIdRequired", "Attendee ID is required for family member tickets.");
    public static Error NotAFamilyMember(string name) => Error.Validation("Event.NotAFamilyMember", $"{name} is not recognized as a registered family member for your membership.");
    public static Error PublicUserCannotInviteOthers => Error.Validation("Event.PublicUserCannotInviteOthers", "Non-members can only register for themselves and cannot add other attendees.");
    public static Error TicketCategoryMismatch(string expectedCategory, string actualCategory) => Error.Validation("Event.TicketCategoryMismatch", $"Ticket category mismatch: expected a '{expectedCategory}' ticket but got a '{actualCategory}' ticket.");
    public static Error DuplicateAttendees => Error.Validation("Event.DuplicateAttendees", "Duplicate attendee IDs found in the registration request.");
    public static Error AlreadyRegistered(string attendeeName) => Error.Validation("Event.AlreadyRegistered", $"Attendee {attendeeName} is already registered for this event.");
    public static Error MaxPerMemberExceeded(string ticketName, int maxPerMember) => Error.Validation("Event.MaxPerMemberExceeded", $"Registrant limits exceeded for ticket '{ticketName}'. Maximum allowed is {maxPerMember}.");
    public static Error RegistrationNotFound => Error.NotFound("Event.RegistrationNotFound", "Registration not found.");
    public static Error AgeRestriction(string ticketName, int min, int max) => Error.Validation("Event.AgeRestriction", $"Attendee age must be between {min} and {max} for ticket '{ticketName}'.");
    public static Error GenderRestriction(string ticketName, string restriction) => Error.Validation("Event.GenderRestriction", $"Ticket '{ticketName}' is restricted to {restriction} attendees.");
    
    public static Error InvalidAge(string attendeeName, string message) => Error.Validation("Event.InvalidAge", $"Attendee {attendeeName}: {message}");
    public static Error InvalidGender(string attendeeName, string restriction) => Error.Validation("Event.InvalidGender", $"Attendee {attendeeName} does not match the gender restriction: {restriction}");
    
    // Publish / Cancel Errors
    public static Error AlreadyPublished => Error.Validation("Event.AlreadyPublished", "Event is not in Draft state.");
    public static Error NoTickets => Error.Validation("Event.NoTickets", "Cannot publish event without at least one ticket type.");
    public static Error AlreadyCancelled => Error.Validation("Event.AlreadyCancelled", "Event is already cancelled.");

    // TicketType Errors
    public static Error InvalidTicketPrice => Error.Validation("TicketType.InvalidPrice", "Ticket price cannot be negative.");
    public static Error InvalidTicketQuantity => Error.Validation("TicketType.InvalidQuantity", "Ticket quantity must be greater than zero.");
    public static Error InvalidMaxPerMember => Error.Validation("TicketType.InvalidMaxPerMember", "Max per member must be greater than zero and less than or equal to ticket quantity.");
    public static Error InvalidReserveCount => Error.Validation("TicketType.InvalidReserveCount", "Must reserve at least one seat.");
    public static Error NotEnoughSeats(string ticketName) => Error.Validation("TicketType.NotEnoughSeats", $"Not enough '{ticketName}' tickets available.");
    public static Error InvalidReleaseCount => Error.Validation("TicketType.InvalidReleaseCount", "Must release at least one seat.");
    public static Error InvalidRelease => Error.Validation("TicketType.InvalidRelease", "Cannot release more seats than have been sold.");
    public static Error PublicTicketCannotRequireMembership => Error.Validation("TicketType.PublicTicketCannotRequireMembership", "Public or Guest tickets cannot require membership.");
    public static Error MemberTicketMustRequireMembership => Error.Validation("TicketType.MemberTicketMustRequireMembership", "Member, Staff, or Spouse tickets must require membership.");

    // EventRegistration Errors
    public static Error NotPendingPayment => Error.Validation("EventRegistration.NotPendingPayment", "Can only attach invoice to a pending registration.");
    public static Error InvalidStateTransition(string message) => Error.Validation("EventRegistration.InvalidStateTransition", message);
}
