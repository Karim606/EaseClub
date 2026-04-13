using System;
using System.Collections.Generic;
using System.Linq;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Events.DomainEvents;
using EaseClub.Domain.Events.Entities;
using EaseClub.Domain.Events.Enums;
using EaseClub.Domain.Events.ValueObjects;

namespace EaseClub.Domain.Events;

public class Event : AuditableEntity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public int Capacity { get; private set; }
    public Audience Audience { get; private set; }
    public EventStatus Status { get; private set; }

    private readonly List<TicketType> _ticketTypes = new();
    public IReadOnlyCollection<TicketType> TicketTypes => _ticketTypes.AsReadOnly();

    private readonly List<EventRegistration> _registrations = new();
    public IReadOnlyCollection<EventRegistration> Registrations => _registrations.AsReadOnly();

    private Event() { }

    public static Result<Event> Create(
        string name, 
        string description, 
        DateTime startDate, 
        DateTime endDate, 
        int capacity, 
        Audience audience)
    {
        if (startDate < DateTime.UtcNow)
            return Error.Validation("Event.PastDate", "Event start date must be in the future.");
            
        if (endDate <= startDate)
            return Error.Validation("Event.InvalidEndDate", "End date must be after start date.");
            
        if (capacity <= 0)
            return Error.Validation("Event.InvalidCapacity", "Event capacity must be greater than zero.");

        var @event = new Event
        {
            Name = name,
            Description = description,
            StartDate = startDate,
            EndDate = endDate,
            Capacity = capacity,
            Audience = audience,
            Status = EventStatus.Draft
        };

        return @event;
    }

    public Result<TicketType> AddTicketType(
        string name, 
        string description, 
        AttendeeCategory category, 
        decimal price, 
        int quantity, 
        int? maxPerMember = null)
    {
        if (Status != EventStatus.Draft)
            return Error.Validation("Event.NotDraft", "Can only add tickets to draft events.");

        // Invariants
        if (price < 0)
            return Error.Validation("TicketType.InvalidPrice", "Ticket price cannot be negative.");
            
        if (quantity <= 0)
            return Error.Validation("TicketType.InvalidQuantity", "Ticket quantity must be greater than zero.");
            
        if (maxPerMember.HasValue && (maxPerMember.Value <= 0 || maxPerMember.Value > quantity))
            return Error.Validation("TicketType.InvalidMaxPerMember", "Max per member must be greater than zero and less than or equal to ticket quantity.");

        // Audience rules enforcement
        var rules = AudienceRules.For(Audience);
        if (!rules.AllowedCategories.Contains(category))
            return Error.Validation("Event.InvalidTicketCategory", $"Category {category} is not allowed for audience {Audience}.");

        // Capacity check
        var currentTotalQuantity = _ticketTypes.Sum(t => t.Quantity);
        if (currentTotalQuantity + quantity > Capacity)
            return Error.Validation("Event.CapacityExceeded", "Adding this ticket type would exceed the total event capacity.");

        var ticket = new TicketType(Id, name, description, category, price, quantity, maxPerMember);
        _ticketTypes.Add(ticket);
        
        return ticket;
    }

    public Result<Success> RemoveTicketType(Guid ticketTypeId)
    {
        if (Status != EventStatus.Draft)
            return Error.Validation("Event.NotDraft", "Can only remove tickets from draft events.");

        var ticket = _ticketTypes.FirstOrDefault(t => t.Id == ticketTypeId);
        if (ticket is null)
            return Error.NotFound("Event.TicketNotFound", "Ticket type not found.");

        _ticketTypes.Remove(ticket);
        return Result.Success;
    }

    public Result<Success> UpdateTicketType(
        Guid ticketTypeId, 
        string name, 
        string description, 
        decimal price, 
        int quantity, 
        int? maxPerMember = null)
    {
        if (Status != EventStatus.Draft)
            return Error.Validation("Event.NotDraft", "Can only update tickets in draft events.");

        var ticket = _ticketTypes.FirstOrDefault(t => t.Id == ticketTypeId);
        if (ticket is null)
            return Error.NotFound("Event.TicketNotFound", "Ticket type not found.");

        if (price < 0)
            return Error.Validation("TicketType.InvalidPrice", "Ticket price cannot be negative.");
            
        if (quantity <= 0)
            return Error.Validation("TicketType.InvalidQuantity", "Ticket quantity must be greater than zero.");
            
        if (maxPerMember.HasValue && (maxPerMember.Value <= 0 || maxPerMember.Value > quantity))
            return Error.Validation("TicketType.InvalidMaxPerMember", "Max per member must be greater than zero and less than or equal to ticket quantity.");

        var otherTicketsQuantity = _ticketTypes.Where(t => t.Id != ticketTypeId).Sum(t => t.Quantity);
        if (otherTicketsQuantity + quantity > Capacity)
            return Error.Validation("Event.CapacityExceeded", "Updating this ticket type would exceed the total event capacity.");

        ticket.UpdateDetails(name, description, price, quantity, maxPerMember);
        return Result.Success;
    }

    public Result<Success> ChangeAudience(Audience newAudience)
    {
        if (Status != EventStatus.Draft)
            return Error.Validation("Event.NotDraft", "Can only change audience for draft events.");

        var newRules = AudienceRules.For(newAudience);
        
        foreach (var ticket in _ticketTypes)
        {
            if (!newRules.AllowedCategories.Contains(ticket.Category))
            {
                return Error.Validation("Event.IncompatibleAudience", 
                    $"Cannot change audience to {newAudience} because existing ticket '{ticket.Name}' has category '{ticket.Category}' which is not allowed.");
            }
        }

        Audience = newAudience;
        return Result.Success;
    }

    public Result<EventRegistration> Register(
        Guid registrantId,
        bool isRegistrantMember,
        List<AttendeeRequest> attendees)
    {
        if (Status != EventStatus.Published)
            return Error.Validation("Event.NotPublished", "Cannot register for an event that is not published.");

        if (attendees == null || !attendees.Any())
            return Error.Validation("Event.NoAttendees", "At least one attendee is required to register.");

        var rules = AudienceRules.For(Audience);
        if (rules.RequiresMemberRegistrant && !isRegistrantMember)
            return Error.Validation("Event.RegistrantMustBeMember", "This event requires the registrant to be a club member.");

        // Rule: Duplicate Attendee ID check - an attendee can only be registered once for the entire event
        var groupedIds = attendees
            .Where(a => a.AttendeeId.HasValue)
            .GroupBy(a => a.AttendeeId!.Value);
            
        if (groupedIds.Any(g => g.Count() > 1))
            return Error.Validation("Event.DuplicateAttendees", "Duplicate attendee IDs found in the registration request.");

        foreach (var req in attendees.Where(a => a.AttendeeId.HasValue))
        {
            bool alreadyRegistered = _registrations
                .Where(r => r.Status != RegistrationStatus.Cancelled)
                .SelectMany(r => r.Attendees)
                .Any(a => a.AttendeeId == req.AttendeeId!.Value);

            if (alreadyRegistered)
                return Error.Validation("Event.AlreadyRegistered", $"Attendee {req.AttendeeName ?? req.AttendeeId.ToString()} is already registered for this event.");
        }

        var ticketRequests = attendees.GroupBy(a => a.TicketTypeId).ToDictionary(g => g.Key, g => g.Count());
        decimal totalBasePrice = 0;

        foreach (var ticketGroup in ticketRequests)
        {
            var ticketTypeId = ticketGroup.Key;
            var requestedQuantity = ticketGroup.Value;
            var ticketType = _ticketTypes.FirstOrDefault(t => t.Id == ticketTypeId);
            
            if (ticketType == null)
                return Error.Validation("Event.TicketNotFound", $"Ticket type not found on this event.");

            // Consume capacity natively
            var reserveResult = ticketType.ReserveSeats(requestedQuantity);
            if (reserveResult.IsError)
                return reserveResult.TopError;

            // Check Max Per Member correctly (Registrant restrictions)
            if (ticketType.MaxPerMember.HasValue)
            {
                var previousRegistrantTicketsForType = _registrations
                    .Where(r => r.RegistrantId == registrantId && r.Status != RegistrationStatus.Cancelled)
                    .SelectMany(r => r.Attendees)
                    .Count(a => a.TicketTypeId == ticketTypeId);

                if (previousRegistrantTicketsForType + requestedQuantity > ticketType.MaxPerMember.Value)
                {
                    // Rollback reservations
                    ticketType.ReleaseSeats(requestedQuantity);
                    return Error.Validation("Event.MaxPerMemberExceeded", 
                        $"Registrant limits exceeded for ticket '{ticketType.Name}'. Maximum allowed is {ticketType.MaxPerMember.Value}.");
                }
            }

            totalBasePrice += (ticketType.Price * requestedQuantity);
        }

        var newRegistration = new EventRegistration(Id, registrantId, totalBasePrice);
        foreach (var req in attendees)
        {
            newRegistration.AddAttendee(new Attendee(newRegistration.Id, req.TicketTypeId, req.AttendeeId, req.AttendeeName));
        }

        _registrations.Add(newRegistration);
        RaiseDomainEvent(new EventRegistrationCreated(newRegistration.Id, Id));

        return newRegistration;
    }

    public Result<Success> ConfirmRegistration(Guid registrationId)
    {
        var registration = _registrations.FirstOrDefault(r => r.Id == registrationId);
        if (registration == null)
            return Error.NotFound("Event.RegistrationNotFound", "Registration not found.");

        var result = registration.MarkAsConfirmed();
        if (result.IsError)
            return result.TopError;

        RaiseDomainEvent(new EventRegistrationConfirmed(registration.Id, Id));
        return Result.Success;
    }

    public Result<Success> CancelRegistration(Guid registrationId)
    {
        var registration = _registrations.FirstOrDefault(r => r.Id == registrationId);
        if (registration == null)
            return Error.NotFound("Event.RegistrationNotFound", "Registration not found.");

        var result = registration.Cancel();
        if (result.IsError)
            return result.TopError;

        // Release the seats!
        var ticketCounts = registration.Attendees.GroupBy(a => a.TicketTypeId).ToDictionary(g => g.Key, g => g.Count());
        foreach (var tc in ticketCounts)
        {
            var ticketType = _ticketTypes.First(t => t.Id == tc.Key);
            ticketType.ReleaseSeats(tc.Value);
        }

        RaiseDomainEvent(new EventRegistrationCancelled(registration.Id, Id));
        return Result.Success;
    }

    public Result<Success> Publish()
    {
        if (Status != EventStatus.Draft)
            return Error.Validation("Event.AlreadyPublished", "Event is not in Draft state.");

        if (!_ticketTypes.Any())
            return Error.Validation("Event.NoTickets", "Cannot publish event without at least one ticket type.");

        if (StartDate <= DateTime.UtcNow)
            return Error.Validation("Event.PastStartDate", "Event start date must be in the future.");

        Status = EventStatus.Published;
        RaiseDomainEvent(new EventPublished(Id));
        return Result.Success;
    }

    public Result<Success> Cancel()
    {
        if (Status == EventStatus.Cancelled)
            return Error.Validation("Event.AlreadyCancelled", "Event is already cancelled.");

        Status = EventStatus.Cancelled;
        
        foreach(var reg in _registrations.Where(r => r.Status != RegistrationStatus.Cancelled))
        {
            reg.Cancel();

            // Release the seats!
            var ticketCounts = reg.Attendees.GroupBy(a => a.TicketTypeId).ToDictionary(g => g.Key, g => g.Count());
            foreach (var tc in ticketCounts)
            {
                var ticketType = _ticketTypes.First(t => t.Id == tc.Key);
                ticketType.ReleaseSeats(tc.Value);
            }

            RaiseDomainEvent(new EventRegistrationCancelled(reg.Id, Id));
        }

        RaiseDomainEvent(new EventCancelled(Id));
        return Result.Success;
    }
}
