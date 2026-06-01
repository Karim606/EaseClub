using System;
using System.Collections.Generic;
using System.Linq;
using EaseClub.Domain.Events.ValueObjects;
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
using EaseClub.Domain.Clubs;

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
    public Club Club { get; private set; }

    private Event() { }

    private Event(Guid id) : base(id) { }

    public static Result<Event> Create(
        Guid clubId,
        string name, 
        string description, 
        DateTime startDate, 
        DateTime endDate, 
        int capacity, 
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
            AccessType = EventAccessType.MembersOnly,
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
        int? minAge = null,
        int? maxAge = null,
        string? genderRestriction = null)
    {
        if (Status != EventStatus.Draft)
            return EventErrors.NotDraft("add tickets to");

        if (category == AttendeeCategory.Public)
        {
            AccessType = EventAccessType.Public;
        }

        // Validate category is compatible with event access type
        var rules = AccessRules.For(AccessType);
        if (!rules.IsCategoryAllowed(category))
            return EventErrors.InvalidTicketCategory(category.ToString(), AccessType.ToString());

        if (_ticketTypes.Any(t => t.Category == category))
            return EventErrors.DuplicateTicketCategory(category.ToString());
 
        // Invariants
        if (basePrice < 0)
            return EventErrors.InvalidTicketPrice;

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

        if (!_ticketTypes.Any(t => t.Category == AttendeeCategory.Public))
        {
            AccessType = EventAccessType.MembersOnly;
        }

        return Result.Success;
    }

    public Result<Success> UpdateTicketType(
        Guid ticketTypeId, 
        AttendeeCategory category,
        decimal basePrice, 
        int totalQuantity, 
        int? maxPerMember = null,
        int? minAge = null,
        int? maxAge = null,
        string? genderRestriction = null)
    {
        if (Status != EventStatus.Draft)
            return EventErrors.NotDraft("update tickets in");
 
        var ticket = _ticketTypes.FirstOrDefault(t => t.Id == ticketTypeId);
        if (ticket is null)
            return EventErrors.TicketNotFound;

        if (category == AttendeeCategory.Public)
        {
            AccessType = EventAccessType.Public;
        }

        // Validate category is compatible with event access type
        var rules = AccessRules.For(AccessType);
        if (!rules.IsCategoryAllowed(category))
            return EventErrors.InvalidTicketCategory(category.ToString(), AccessType.ToString());
 
        if (ticket.Category != category && _ticketTypes.Any(t => t.Id != ticketTypeId && t.Category == category))
            return EventErrors.DuplicateTicketCategory(category.ToString());

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
            category,
            basePrice, 
            totalQuantity, 
            maxPerMember,
            minAge,
            maxAge,
            genderRestriction);

        if (result.IsError)
            return result.TopError;

        if (!_ticketTypes.Any(t => t.Category == AttendeeCategory.Public))
        {
            AccessType = EventAccessType.MembersOnly;
        }

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
        IReadOnlyCollection<AttendeeRequest> attendees,
        IReadOnlyCollection<Guid> familyMemberIds)
    {
        var context = BuildContext(
            registrantId,
            isRegistrantMember,
            isRegistrantAttending,
            registrantName,
            registrantAge,
            registrantGender,
            attendees,
            familyMemberIds);

        var validation = ValidateRegistration(context);
        if (validation.IsError)
            return validation.TopError;

        ReserveSeats(context);

        var registration = CreateRegistration(context);

        _registrations.Add(registration);

        RaiseDomainEvent(
            new EventRegistrationCreated(registration.Id, Id));

        return registration;
    }

    private Result<Success> ValidateRegistration(RegistrationContext context)
    {
        var result = ValidateEventState(context);
        if (result.IsError) return result;

        result = ValidateRegistrant(context);
        if (result.IsError) return result;

        result = ValidateAttendees(context);
        if (result.IsError) return result;

        return ValidateCapacity(context);
    }

    private Result<Success> ValidateEventState(RegistrationContext context)
    {
        if (Status != EventStatus.Published)
            return EventErrors.NotPublished;

        if (StartDate <= DateTime.UtcNow)
            return EventErrors.EventAlreadyStarted;

        var rules = AccessRules.For(AccessType);

        if (rules.RequiresMemberRegistrant && !context.IsMember)
            return EventErrors.RegistrantMustBeMember;

        if (!context.IsAttending && !context.Attendees.Any())
            return EventErrors.NoAttendees;

        return Result.Success;
    }

    private Result<Success> ValidateRegistrant(RegistrationContext context)
    {
        if (!context.IsAttending)
            return Result.Success;

        var category = context.IsMember
            ? (_ticketTypes.Any(t => t.Category == AttendeeCategory.Member) ? AttendeeCategory.Member : AttendeeCategory.Public)
            : AttendeeCategory.Public;

        var ticket = _ticketTypes.FirstOrDefault(t => t.Category == category);

        if (ticket == null)
            return EventErrors.TicketNotFoundForCategory(category.ToString());

        context.RegistrantTicket = ticket;

        if (IsAlreadyRegistered(context.RegistrantId))
            return EventErrors.AlreadyRegistered(context.RegistrantName);

        var restriction = ValidateTicketRestrictions(
            ticket,
            context.Age,
            context.Gender);

        if (restriction.IsError)
            return restriction;

        if (ticket.AvailableQuantity < 1)
            return EventErrors.NotEnoughSeats(ticket.Category.ToString());

        return Result.Success;
    }

    private bool IsAlreadyRegistered(Guid attendeeId)
    {
        return _registrations
            .Where(r => r.Status != RegistrationStatus.Cancelled)
            .SelectMany(r => r.Attendees)
            .Any(a => a.AttendeeId == attendeeId);
    }

    private Result<Success> ValidateAttendees(RegistrationContext context)
    {
        var duplicateIds = context.Attendees
            .Where(a => a.AttendeeId.HasValue)
            .GroupBy(a => a.AttendeeId!.Value)
            .Any(g => g.Count() > 1);

        if (duplicateIds)
            return EventErrors.DuplicateAttendees;

        foreach (var attendee in context.Attendees)
        {
            if (!context.TicketLookup.TryGetValue(attendee.TicketTypeId, out var ticket))
                return EventErrors.TicketNotFound;

            var membership = ValidateMembershipRules(context, attendee, ticket);
            if (membership.IsError) return membership;

            var restriction = ValidateTicketRestrictions(ticket, attendee.Age, attendee.Gender);
            if (restriction.IsError) return restriction;

            if (attendee.AttendeeId.HasValue &&
                IsAlreadyRegistered(attendee.AttendeeId.Value))
            {
                return EventErrors.AlreadyRegistered(attendee.AttendeeName ?? "");
            }
        }

        return Result.Success;
    }

    private Result<Success> ValidateMembershipRules(
        RegistrationContext context,
        AttendeeRequest attendee,
        TicketType ticket)
    {
        if (!ticket.RequiresMembership)
            return Result.Success;

        if (!context.IsMember)
            return EventErrors.MemberTicketRequired;

        if (ticket.Category == AttendeeCategory.Member &&
            attendee.AttendeeId != context.RegistrantId)
            return EventErrors.MemberTicketRequired;

        if (ticket.Category == AttendeeCategory.FamilyMember)
        {
            if (!attendee.AttendeeId.HasValue)
                return EventErrors.AttendeeIdRequired;

            if (!context.FamilyMemberIds.Contains(attendee.AttendeeId.Value))
                return EventErrors.NotAFamilyMember(attendee.AttendeeName ?? "Attendee");
        }

        return Result.Success;
    }

    private Result<Success> ValidateTicketRestrictions(
        TicketType ticket,
        int? age,
        string? gender)
    {
        if (ticket.MinAge.HasValue &&
            (!age.HasValue || age < ticket.MinAge.Value))
        {
            return EventErrors.AgeRestriction(
                ticket.Category.ToString(),
                ticket.MinAge.Value,
                ticket.MaxAge ?? 99);
        }

        if (ticket.MaxAge.HasValue &&
            (!age.HasValue || age > ticket.MaxAge.Value))
        {
            return EventErrors.AgeRestriction(
                ticket.Category.ToString(),
                ticket.MinAge ?? 0,
                ticket.MaxAge.Value);
        }

        if (!string.IsNullOrEmpty(ticket.GenderRestriction) &&
            !string.Equals(gender, ticket.GenderRestriction, StringComparison.OrdinalIgnoreCase))
        {
            return EventErrors.GenderRestriction(
                ticket.Category.ToString(),
                ticket.GenderRestriction);
        }

        return Result.Success;
    }

    private Result<Success> ValidateCapacity(RegistrationContext context)
    {
        var grouped = context.Attendees.GroupBy(a => a.TicketTypeId);

        foreach (var group in grouped)
        {
            var ticket = context.TicketLookup[group.Key];

            if (ticket.AvailableQuantity < group.Count())
                return EventErrors.NotEnoughSeats(ticket.Category.ToString());

            if (ticket.MaxPerMember.HasValue)
            {
                var previous = _registrations
                    .Where(r => r.RegistrantId == context.RegistrantId &&
                                r.Status != RegistrationStatus.Cancelled)
                    .SelectMany(r => r.Attendees)
                    .Count(a => a.TicketTypeId == group.Key);

                if (previous + group.Count() > ticket.MaxPerMember.Value)
                    return EventErrors.MaxPerMemberExceeded(
                        ticket.Category.ToString(),
                        ticket.MaxPerMember.Value);
            }
        }

        return Result.Success;
    }

    private void ReserveSeats(RegistrationContext context)
    {
        if (context.RegistrantTicket != null)
        {
            context.RegistrantTicket.ReserveSeats(1);
            context.TotalBasePrice += context.RegistrantTicket.BasePrice;
        }

        foreach (var group in context.Attendees.GroupBy(a => a.TicketTypeId))
        {
            var ticket = context.TicketLookup[group.Key];

            ticket.ReserveSeats(group.Count());

            context.TotalBasePrice += ticket.BasePrice * group.Count();
        }
    }

    private EventRegistration CreateRegistration(RegistrationContext context)
    {
        var registration = new EventRegistration(
            Id,
            context.RegistrantId,
            context.IsAttending,
            context.TotalBasePrice);

        var readableId = BillingITemIdGenerator.Generate(
            BillingItemType.EventRegistration,
            context.RegistrantId,
            ClubId,
            registration.Id,
            StartDate
        );
        registration.SetReadableId(readableId);

        if (context.IsAttending && context.RegistrantTicket != null)
        {
            registration.AddAttendee(
                new Attendee(
                    registration.Id,
                    context.RegistrantTicket.Id,
                    context.RegistrantId,
                    context.RegistrantName,
                    context.Age,
                    context.Gender));
        }

        foreach (var attendee in context.Attendees)
        {
            registration.AddAttendee(
                new Attendee(
                    registration.Id,
                    attendee.TicketTypeId,
                    attendee.AttendeeId,
                    attendee.AttendeeName,
                    attendee.Age,
                    attendee.Gender));
        }

        return registration;
    }

    private sealed class RegistrationContext
    {
        public Guid RegistrantId { get; init; }
        public bool IsMember { get; init; }
        public bool IsAttending { get; init; }
        public string RegistrantName { get; init; }
        public int? Age { get; init; }
        public string? Gender { get; init; }

        public IReadOnlyCollection<AttendeeRequest> Attendees { get; init; }
        public IReadOnlyCollection<Guid> FamilyMemberIds { get; init; }

        public Dictionary<Guid, TicketType> TicketLookup { get; init; } = new();

        public TicketType? RegistrantTicket { get; set; }
        public decimal TotalBasePrice { get; set; }
    }

    private RegistrationContext BuildContext(
        Guid registrantId,
        bool isRegistrantMember,
        bool isRegistrantAttending,
        string registrantName,
        int? registrantAge,
        string? registrantGender,
        IReadOnlyCollection<AttendeeRequest> attendees,
        IReadOnlyCollection<Guid> familyMemberIds)
    {
        var normalizedAttendees = attendees?.ToList() ?? [];

        if (isRegistrantAttending)
        {
            normalizedAttendees = normalizedAttendees
                .Where(a => a.AttendeeId != registrantId)
                .ToList();
        }

        return new RegistrationContext
        {
            RegistrantId = registrantId,
            IsMember = isRegistrantMember,
            IsAttending = isRegistrantAttending,
            RegistrantName = registrantName,
            Age = registrantAge,
            Gender = registrantGender,
            Attendees = normalizedAttendees,
            FamilyMemberIds = familyMemberIds ?? Array.Empty<Guid>(),
            TicketLookup = _ticketTypes.ToDictionary(t => t.Id)
        };
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
            var ticketType = _ticketTypes.FirstOrDefault(t => t.Id == tc.Key);
            if (ticketType != null)
            {
                ticketType.ReleaseSeats(tc.Value);
            }
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

