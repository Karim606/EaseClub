using System;
using System.Collections.Generic;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Events.Enums;

namespace EaseClub.Domain.Events.Entities;

public class EventRegistration : AuditableEntity
{
    public Guid EventId { get; private set; }
    public Guid RegistrantId { get; private set; }
    public RegistrationStatus Status { get; private set; }
    public Guid? InvoiceId { get; private set; }
    
    // Snapshot of the calculated base price across all tickets in this registration
    public decimal TotalBasePrice { get; private set; }

    private readonly List<Attendee> _attendees = new();
    public IReadOnlyCollection<Attendee> Attendees => _attendees.AsReadOnly();

    internal EventRegistration(Guid eventId, Guid registrantId, decimal totalBasePrice) : base(Guid.NewGuid())
    {
        EventId = eventId;
        RegistrantId = registrantId;
        TotalBasePrice = totalBasePrice;
        Status = RegistrationStatus.PendingPayment;
    }

    private EventRegistration() { }

    internal void AddAttendee(Attendee attendee)
    {
        _attendees.Add(attendee);
    }

    public Result<Success> SetInvoiceId(Guid invoiceId)
    {
        if (Status != RegistrationStatus.PendingPayment)
            return EventErrors.NotPendingPayment;

        InvoiceId = invoiceId;
        return Result.Success;
    }

    internal Result<Success> MarkAsConfirmed()
    {
        // Must strictly transition from PendingPayment -> Confirmed
        if (Status != RegistrationStatus.PendingPayment)
            return EventErrors.InvalidStateTransition($"Cannot transition to Confirmed from {Status}.");

        Status = RegistrationStatus.Confirmed;
        return Result.Success;
    }

    internal Result<Success> Cancel()
    {
        // Can cancel from PendingPayment or Confirmed
        if (Status == RegistrationStatus.Cancelled)
            return EventErrors.InvalidStateTransition("Registration is already cancelled.");

        Status = RegistrationStatus.Cancelled;
        return Result.Success;
    }
}
