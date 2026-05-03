using System;
using System.Collections.Generic;
using System.Linq;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Events.DomainEvents;
using EaseClub.Domain.Events.Entities;
using EaseClub.Domain.Events.Enums;
using EaseClub.Domain.Events.ValueObjects;

namespace EaseClub.Domain.Events;

public class Event : AuditableEntity, IHaveClub
{
    public Guid ClubId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public int Capacity { get; private set; }
    public Audience Audience { get; private set; }
    public EventStatus Status { get; private set; }
    
    // UI/Display Properties matching App requirements
    public string Venue { get; private set; } = string.Empty;
    public string? ImageUrl { get; private set; }
    public string? Badge { get; private set; }
    public bool IsFeatured { get; private set; }

    private readonly List<TicketType> _ticketTypes = new();
    public IReadOnlyCollection<TicketType> TicketTypes => _ticketTypes.AsReadOnly();

    private readonly List<EventRegistration> _registrations = new();
    public IReadOnlyCollection<EventRegistration> Registrations => _registrations.AsReadOnly();

    private readonly List<Guid> _pricingPolicyIds = new();
    public IReadOnlyCollection<Guid> PricingPolicyIds => _pricingPolicyIds.AsReadOnly();

    private Event() { }

    private Event(Guid id) : base(id) { }

    public static Result<Event> Create(
        Guid clubId,
        string name, 
        string description, 
        DateTime startDate, 
        DateTime endDate, 
        int capacity, 
        Audience audience,
        string venue = "",
        string? imageUrl = null,
        string? badge = null,
        bool isFeatured = false)
    {
        if (clubId == Guid.Empty)
            return EventErrors.InvalidClub;

        if (startDate < DateTime.UtcNow)
            return EventErrors.PastDate;
            
        if (endDate <= startDate)
            return EventErrors.InvalidEndDate;
            
        if (capacity <= 0)
            return EventErrors.InvalidCapacity;

        var @event = new Event(Guid.NewGuid())
        {
            ClubId = clubId,
            Name = name,
            Description = description,
            StartDate = startDate,
            EndDate = endDate,
            Capacity = capacity,
            Audience = audience,
            Status = EventStatus.Draft,
            Venue = venue ?? string.Empty,
            ImageUrl = imageUrl,
            Badge = badge,
            IsFeatured = isFeatured
        };

        return @event;
    }

    public Result<Success> UpdateDetails(
        string name, 
        string description, 
        DateTime startDate, 
        DateTime endDate, 
        int capacity,
        string venue = "",
        string? imageUrl = null,
        string? badge = null,
        bool isFeatured = false)
    {
        if (Status != EventStatus.Draft)
            return EventErrors.NotDraft("update");

        if (startDate < DateTime.UtcNow)
            return EventErrors.PastDate;

        if (endDate <= startDate)
            return EventErrors.InvalidEndDate;

        if (capacity <= 0)
            return EventErrors.InvalidCapacity;

        // Check if new capacity can accommodate existing ticket types
        var totalTicketQuantity = _ticketTypes.Sum(t => t.TotalQuantity);
        if (capacity < totalTicketQuantity)
            return EventErrors.CapacityTooSmall;

        Name = name;
        Description = description;
        StartDate = startDate;
        EndDate = endDate;
        Capacity = capacity;
        Venue = venue ?? string.Empty;
        ImageUrl = imageUrl;
        Badge = badge;
        IsFeatured = isFeatured;

        return Result.Success;
    }

    public Result<TicketType> AddTicketType(
        string name, 
        string description, 
        AttendeeCategory category, 
        decimal basePrice, 
        int totalQuantity, 
        int? maxPerMember = null,
        bool requiresMembership = false,
        int? minAge = null,
        int? maxAge = null,
        string? genderRestriction = null)
    {
        if (Status != EventStatus.Draft)
            return EventErrors.NotDraft("add tickets to");
 
        // Invariants
        if (basePrice < 0)
            return EventErrors.InvalidTicketPrice;
            
        if (totalQuantity <= 0)
            return EventErrors.InvalidTicketQuantity;
            
        if (maxPerMember.HasValue && (maxPerMember.Value <= 0 || maxPerMember.Value > totalQuantity))
            return EventErrors.InvalidMaxPerMember;
 
        // Audience rules enforcement
        var rules = AudienceRules.For(Audience);
        if (!rules.AllowedCategories.Contains(category))
            return EventErrors.InvalidTicketCategory(category.ToString(), Audience.ToString());
 
        // Capacity check
        var currentTotalQuantity = _ticketTypes.Sum(t => t.TotalQuantity);
        if (currentTotalQuantity + totalQuantity > Capacity)
            return EventErrors.CapacityExceeded;
 
        var ticket = new TicketType(
            Id, 
            name, 
            description, 
            category, 
            basePrice, 
            totalQuantity, 
            maxPerMember,
            requiresMembership,
            minAge,
            maxAge,
            genderRestriction);
        _ticketTypes.Add(ticket);
        
        return ticket;
    }

    public Result<Success> RemoveTicketType(Guid ticketTypeId)
    {
        if (Status != EventStatus.Draft)
            return EventErrors.NotDraft("remove tickets from");

        var ticket = _ticketTypes.FirstOrDefault(t => t.Id == ticketTypeId);
        if (ticket is null)
            return EventErrors.TicketNotFound;

        _ticketTypes.Remove(ticket);
        return Result.Success;
    }

    public Result<Success> UpdateTicketType(
        Guid ticketTypeId, 
        string name, 
        string description, 
        decimal basePrice, 
        int totalQuantity, 
        int? maxPerMember = null,
        bool requiresMembership = false,
        int? minAge = null,
        int? maxAge = null,
        string? genderRestriction = null)
    {
        if (Status != EventStatus.Draft)
            return EventErrors.NotDraft("update tickets in");
 
        var ticket = _ticketTypes.FirstOrDefault(t => t.Id == ticketTypeId);
        if (ticket is null)
            return EventErrors.TicketNotFound;
 
        if (basePrice < 0)
            return EventErrors.InvalidTicketPrice;
            
        if (totalQuantity <= 0)
            return EventErrors.InvalidTicketQuantity;
            
        if (maxPerMember.HasValue && (maxPerMember.Value <= 0 || maxPerMember.Value > totalQuantity))
            return EventErrors.InvalidMaxPerMember;
 
        var otherTicketsQuantity = _ticketTypes.Where(t => t.Id != ticketTypeId).Sum(t => t.TotalQuantity);
        if (otherTicketsQuantity + totalQuantity > Capacity)
            return EventErrors.CapacityExceeded;
 
        ticket.UpdateDetails(
            name, 
            description, 
            basePrice, 
            totalQuantity, 
            maxPerMember,
            requiresMembership,
            minAge,
            maxAge,
            genderRestriction);
        return Result.Success;
    }

    public Result<Success> ChangeAudience(Audience newAudience)
    {
        if (Status != EventStatus.Draft)
            return EventErrors.NotDraft("change audience for");

        var newRules = AudienceRules.For(newAudience);
        
        foreach (var ticket in _ticketTypes)
        {
            if (!newRules.AllowedCategories.Contains(ticket.Category))
            {
                return EventErrors.IncompatibleAudience(newAudience.ToString(), ticket.Name, ticket.Category.ToString());
            }
        }

        Audience = newAudience;
        return Result.Success;
    }

    public Result<Success> AssignPricingPolicy(Guid policyId)
    {
        if (Status != EventStatus.Draft)
            return EventErrors.NotDraft("assign policies to");

        if (_pricingPolicyIds.Contains(policyId))
            return Result.Success;

        _pricingPolicyIds.Add(policyId);
        return Result.Success;
    }

    public Result<Success> UnassignPricingPolicy(Guid policyId)
    {
        if (Status != EventStatus.Draft)
            return EventErrors.NotDraft("unassign policies from");

        _pricingPolicyIds.Remove(policyId);
        return Result.Success;
    }

    public Result<EventRegistration> Register(
        Guid registrantId,
        bool isRegistrantMember,
        List<AttendeeRequest> attendees)
    {
        if (Status != EventStatus.Published)
            return EventErrors.NotPublished;

        if (attendees == null || !attendees.Any())
            return EventErrors.NoAttendees;

        var rules = AudienceRules.For(Audience);
        if (rules.RequiresMemberRegistrant && !isRegistrantMember)
            return EventErrors.RegistrantMustBeMember;

        // Rule: Duplicate Attendee ID check - an attendee can only be registered once for the entire event
        var groupedIds = attendees
            .Where(a => a.AttendeeId.HasValue)
            .GroupBy(a => a.AttendeeId!.Value);
            
        if (groupedIds.Any(g => g.Count() > 1))
            return EventErrors.DuplicateAttendees;

        foreach (var req in attendees.Where(a => a.AttendeeId.HasValue))
        {
            bool alreadyRegistered = _registrations
                .Where(r => r.Status != RegistrationStatus.Cancelled)
                .SelectMany(r => r.Attendees)
                .Any(a => a.AttendeeId == req.AttendeeId!.Value);

            if (alreadyRegistered)
                return EventErrors.AlreadyRegistered(req.AttendeeName ?? req.AttendeeId.ToString() ?? "");
        }

        var ticketRequests = attendees.GroupBy(a => a.TicketTypeId).ToDictionary(g => g.Key, g => g.Count());
        decimal totalBasePrice = 0;

        foreach (var ticketGroup in ticketRequests)
        {
            var ticketTypeId = ticketGroup.Key;
            var requestedQuantity = ticketGroup.Value;
            var ticketType = _ticketTypes.FirstOrDefault(t => t.Id == ticketTypeId);
            
            if (ticketType == null)
                return EventErrors.TicketNotFound;

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
                    ticketType.ReleaseSeats(requestedQuantity);
                    return EventErrors.MaxPerMemberExceeded(ticketType.Name, ticketType.MaxPerMember.Value);
                }
            }

            totalBasePrice += (ticketType.BasePrice * requestedQuantity);
        }

        var newRegistration = new EventRegistration(Id, registrantId, totalBasePrice); // TODO: Pricing Logic for DiscountAmount and AppliedPolicies
        foreach (var req in attendees)
        {
            newRegistration.AddAttendee(new Attendee(newRegistration.Id, req.TicketTypeId, req.AttendeeId, req.AttendeeName, req.Age, req.Gender));
        }

        _registrations.Add(newRegistration);
        RaiseDomainEvent(new EventRegistrationCreated(newRegistration.Id, Id));

        return newRegistration;
    }

    public Result<Success> ConfirmRegistration(Guid registrationId)
    {
        var registration = _registrations.FirstOrDefault(r => r.Id == registrationId);
        if (registration == null)
            return EventErrors.RegistrationNotFound;

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
            return EventErrors.RegistrationNotFound;

        var result = registration.Cancel();
        if (result.IsError)
            return result.TopError;

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
            return EventErrors.AlreadyPublished;

        if (!_ticketTypes.Any())
            return EventErrors.NoTickets;

        if (StartDate <= DateTime.UtcNow)
            return EventErrors.PastDate;

        Status = EventStatus.Published;
        RaiseDomainEvent(new EventPublished(Id));
        return Result.Success;
    }

    public Result<Success> Cancel()
    {
        if (Status == EventStatus.Cancelled)
            return EventErrors.AlreadyCancelled;

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
