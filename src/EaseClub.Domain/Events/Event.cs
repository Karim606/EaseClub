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
using EaseClub.Domain.Payment;
using EaseClub.Domain.Payment.Enums;
using EaseClub.Domain.Files;

namespace EaseClub.Domain.Events;

public class Event : AuditableEntity, IHaveClub
{
    public Guid ClubId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public int Capacity { get; private set; }
    public EventAccessType AccessType { get; private set; }
    public EventStatus Status { get; private set; }

    public string Venue { get; private set; } = string.Empty;
    public Guid? ImageId { get; private set; }
    public FileResource? Image { get; private set; }
    public string? Badge { get; private set; }

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
        EventAccessType accessType,
        string venue = "",
        Guid? imageId = null,
        string? badge = null)
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
            AccessType = accessType,
            Status = EventStatus.Draft,
            Venue = venue ?? string.Empty,
            ImageId = imageId,
            Badge = badge
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
        Guid? imageId = null,
        string? badge = null)
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
        ImageId = imageId;
        Badge = badge;

        return Result.Success;
    }

    public void UpdateImageId(Guid? imageId)
    {
        ImageId = imageId;
    }

    public Result<TicketType> AddTicketType(
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

        if (_ticketTypes.Any(t => t.Category == category))
            return EventErrors.DuplicateTicketCategory(category.ToString());
 
        // Invariants
        if (basePrice < 0)
            return EventErrors.InvalidTicketPrice;

        // Consistency check: Category vs RequiresMembership
        if (requiresMembership && (category == AttendeeCategory.Public || category == AttendeeCategory.Guest))
            return EventErrors.PublicTicketCannotRequireMembership;
        
        if (!requiresMembership && (category == AttendeeCategory.Member || category == AttendeeCategory.FamilyMember))
            return EventErrors.MemberTicketMustRequireMembership;
            
        if (totalQuantity <= 0)
            return EventErrors.InvalidTicketQuantity;
            
        if (maxPerMember.HasValue && (maxPerMember.Value <= 0 || maxPerMember.Value > totalQuantity))
            return EventErrors.InvalidMaxPerMember;
        // Capacity check
        var currentTotalQuantity = _ticketTypes.Sum(t => t.TotalQuantity);
        if (currentTotalQuantity + totalQuantity > Capacity)
            return EventErrors.CapacityExceeded;
 
        var ticket = new TicketType(
            Id, 
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
 
        var result = ticket.UpdateDetails(
            basePrice, 
            totalQuantity, 
            maxPerMember,
            requiresMembership,
            minAge,
            maxAge,
            genderRestriction);

        if (result.IsError)
            return result.TopError;

        return Result.Success;
    }

    public Result<Success> ChangeAccessType(EventAccessType newAccessType)
    {
        if (Status != EventStatus.Draft)
            return EventErrors.NotDraft("change access type for");

        AccessType = newAccessType;
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
        bool isRegistrantAttending,
        string registrantName,
        int? registrantAge,
        string? registrantGender,
        IEnumerable<AttendeeRequest> attendees,
        IEnumerable<Guid>? familyMemberIds = null)
    {
        if (Status != EventStatus.Published)
            return EventErrors.NotPublished;

        if (StartDate <= DateTime.UtcNow)
            return EventErrors.EventAlreadyStarted;

        // Access control
        var rules = AccessRules.For(AccessType);
        if (rules.RequiresMemberRegistrant && !isRegistrantMember)
            return EventErrors.RegistrantMustBeMember;

        // Must have at least 1 person
        if (!isRegistrantAttending && (attendees == null || !attendees.Any()))
            return EventErrors.NoAttendees;

        // ─── VALIDATION PHASE (no state changes) ───────────────────

        // Determine registrant ticket
        TicketType? registrantTicket = null;
        if (isRegistrantAttending)
        {
            // Determine registrant ticket category with fallback
            AttendeeCategory registrantCategory;
            if (isRegistrantMember)
            {
                // Try Member ticket first, fallback to Public if no Member ticket exists
                registrantCategory = _ticketTypes.Any(t => t.Category == AttendeeCategory.Member) 
                    ? AttendeeCategory.Member 
                    : AttendeeCategory.Public;
            }
            else
            {
                registrantCategory = AttendeeCategory.Public;
            }

            registrantTicket = _ticketTypes.FirstOrDefault(t => t.Category == registrantCategory);
            if (registrantTicket == null)
                return EventErrors.TicketNotFoundForCategory(registrantCategory.ToString());

            // Check not already registered
            bool alreadyRegistered = _registrations
                .Where(r => r.Status != RegistrationStatus.Cancelled)
                .SelectMany(r => r.Attendees)
                .Any(a => a.AttendeeId == registrantId);
            if (alreadyRegistered)
                return EventErrors.AlreadyRegistered(registrantName);

            // Validate age/gender restrictions
            if (registrantTicket.MinAge.HasValue && (!registrantAge.HasValue || registrantAge < registrantTicket.MinAge.Value))
                return EventErrors.AgeRestriction(registrantTicket.Category.ToString(), registrantTicket.MinAge.Value, registrantTicket.MaxAge ?? 99);
            if (registrantTicket.MaxAge.HasValue && (!registrantAge.HasValue || registrantAge > registrantTicket.MaxAge.Value))
                return EventErrors.AgeRestriction(registrantTicket.Category.ToString(), registrantTicket.MinAge ?? 0, registrantTicket.MaxAge.Value);
            if (!string.IsNullOrEmpty(registrantTicket.GenderRestriction) && !string.Equals(registrantGender, registrantTicket.GenderRestriction, StringComparison.OrdinalIgnoreCase))
                return EventErrors.GenderRestriction(registrantTicket.Category.ToString(), registrantTicket.GenderRestriction);

            // Check availability
            if (registrantTicket.AvailableQuantity < 1)
                return EventErrors.NotEnoughSeats(registrantTicket.Category.ToString());
        }

        // Validate other attendees
        if (attendees != null && attendees.Any())
        {
            // If registrant is attending, we should not have him in the attendees list as well
            if (isRegistrantAttending)
            {
                attendees = attendees.Where(a => a.AttendeeId != registrantId).ToList();
            }

            if (!attendees.Any() && !isRegistrantAttending)
                return EventErrors.NoAttendees;
            // Duplicate attendee ID check
            var groupedIds = attendees
                .Where(a => a.AttendeeId.HasValue)
                .GroupBy(a => a.AttendeeId!.Value);
            if (groupedIds.Any(g => g.Count() > 1))
                return EventErrors.DuplicateAttendees;

            // Check each attendee not already registered
            foreach (var req in attendees.Where(a => a.AttendeeId.HasValue))
            {
                bool alreadyRegistered = _registrations
                    .Where(r => r.Status != RegistrationStatus.Cancelled)
                    .SelectMany(r => r.Attendees)
                    .Any(a => a.AttendeeId == req.AttendeeId!.Value);
                if (alreadyRegistered)
                    return EventErrors.AlreadyRegistered(req.AttendeeName ?? "");
            }

            // Validate age/gender restrictions
            foreach (var req in attendees)
            {
                var ticketType = _ticketTypes.FirstOrDefault(t => t.Id == req.TicketTypeId);
                if (ticketType == null) return EventErrors.TicketNotFound;

                // 1. Membership Check
                if (ticketType.RequiresMembership)
                {
                    // Basic Rule: Only members can purchase tickets that require membership
                    if (!isRegistrantMember)
                        return EventErrors.MemberTicketRequired;

                    // Specific Rule: A 'Member' category ticket is ONLY for the registrant themselves
                    if (ticketType.Category == AttendeeCategory.Member && req.AttendeeId != registrantId)
                        return EventErrors.MemberTicketRequired;
                    
                    // For FamilyMember category: must be in the registrant's family member list
                    if (ticketType.Category == AttendeeCategory.FamilyMember)
                    {
                        if (!req.AttendeeId.HasValue)
                            return EventErrors.AttendeeIdRequired;

                        if (familyMemberIds == null || !familyMemberIds.Contains(req.AttendeeId.Value))
                            return EventErrors.NotAFamilyMember(req.AttendeeName ?? "Attendee");
                    }
                }

                // 2. Age restriction check
                    return EventErrors.AgeRestriction(ticketType.Category.ToString(), ticketType.MinAge.Value, ticketType.MaxAge ?? 99);
                if (ticketType.MaxAge.HasValue && (!req.Age.HasValue || req.Age > ticketType.MaxAge.Value))
                    return EventErrors.AgeRestriction(ticketType.Category.ToString(), ticketType.MinAge ?? 0, ticketType.MaxAge.Value);
                if (!string.IsNullOrEmpty(ticketType.GenderRestriction) && !string.Equals(req.Gender, ticketType.GenderRestriction, StringComparison.OrdinalIgnoreCase))
                    return EventErrors.GenderRestriction(ticketType.Category.ToString(), ticketType.GenderRestriction);
            }

            // Check availability per ticket type
            var ticketRequests = attendees.GroupBy(a => a.TicketTypeId).ToDictionary(g => g.Key, g => g.Count());
            foreach (var ticketGroup in ticketRequests)
            {
                var ticketType = _ticketTypes.FirstOrDefault(t => t.Id == ticketGroup.Key);
                if (ticketType == null) return EventErrors.TicketNotFound;
                if (ticketType.AvailableQuantity < ticketGroup.Value)
                    return EventErrors.NotEnoughSeats(ticketType.Category.ToString());

                // MaxPerMember check
                if (ticketType.MaxPerMember.HasValue)
                {
                    var previousCount = _registrations
                        .Where(r => r.RegistrantId == registrantId && r.Status != RegistrationStatus.Cancelled)
                        .SelectMany(r => r.Attendees)
                        .Count(a => a.TicketTypeId == ticketGroup.Key);
                    if (previousCount + ticketGroup.Value > ticketType.MaxPerMember.Value)
                        return EventErrors.MaxPerMemberExceeded(ticketType.Category.ToString(), ticketType.MaxPerMember.Value);
                }
            }
        }

        // ─── RESERVATION PHASE (state changes) ─────────────────────
        decimal totalBasePrice = 0;

        // Reserve registrant seat
        if (isRegistrantAttending && registrantTicket != null)
        {
            registrantTicket.ReserveSeats(1);
            totalBasePrice += registrantTicket.BasePrice;
        }

        // Reserve attendee seats
        if (attendees != null && attendees.Any())
        {
            var ticketRequests = attendees.GroupBy(a => a.TicketTypeId).ToDictionary(g => g.Key, g => g.Count());
            foreach (var ticketGroup in ticketRequests)
            {
                var ticketType = _ticketTypes.First(t => t.Id == ticketGroup.Key);
                ticketType.ReserveSeats(ticketGroup.Value);
                totalBasePrice += (ticketType.BasePrice * ticketGroup.Value);
            }
        }

        // ─── CREATE REGISTRATION ────────────────────────────────────
        var newRegistration = new EventRegistration(Id, registrantId, isRegistrantAttending, totalBasePrice);
        
        var readableId = BillingITemIdGenerator.Generate(
            BillingItemType.EventRegistration,
            registrantId,
            ClubId,
            newRegistration.Id,
            StartDate
        );
        newRegistration.SetReadableId(readableId);

        // Add registrant as attendee if attending
        if (isRegistrantAttending && registrantTicket != null)
        {
            newRegistration.AddAttendee(new Attendee(
                newRegistration.Id, registrantTicket.Id, registrantId, registrantName, registrantAge, registrantGender));
        }

        // Add other attendees
        if (attendees != null)
        {
            foreach (var req in attendees)
            {
                newRegistration.AddAttendee(new Attendee(
                    newRegistration.Id, req.TicketTypeId, req.AttendeeId, req.AttendeeName, req.Age, req.Gender));
            }
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

    public AttendanceStats GetAttendanceStats()
    {
        var soldPerCategory = _ticketTypes.ToDictionary(t => t.Category, t => t.SoldQuantity);

        return new AttendanceStats(
            Capacity,
            _ticketTypes.Sum(t => t.SoldQuantity),
            _ticketTypes.Sum(t => t.AvailableQuantity),
            soldPerCategory
        );
    }
}

public record AttendanceStats(
    int TotalCapacity,
    int TotalSold,
    int TotalAvailable,
    Dictionary<AttendeeCategory, int> Details);

