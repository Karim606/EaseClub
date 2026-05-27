using System;
using System.Collections.Generic;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Events.Enums;

using EaseClub.Domain.Payment;
using EaseClub.Domain.Payment.Enums;

namespace EaseClub.Domain.Events.Entities;

public class EventRegistration : AuditableEntity, IBillingItem
{
    public Guid EventId { get; private set; }
    public Guid RegistrantId { get; private set; }
    public RegistrationStatus Status { get; private set; }
    public bool IsRegistrantAttending { get; private set; }
    public Guid? InvoiceId { get; private set; }
    
    // Snapshot of the calculated base price across all tickets in this registration
    public decimal TotalBasePrice { get; private set; }
    
    public decimal DiscountAmount { get; private set; }
    public decimal FinalTotal { get; private set; }
    public string? AppliedPolicies { get; private set; }
    
    public string ReadableId { get; private set; } = string.Empty;
    
    public decimal Amount => FinalTotal;

    public BillingItemType GetBillingType() => BillingItemType.EventRegistration;

    public Result<Success> CanBePaid()
    {
        if (Status == RegistrationStatus.Confirmed)
            return EventErrors.InvalidStateTransition("Registration is already confirmed and paid.");

        if (Status == RegistrationStatus.Cancelled)
            return EventErrors.InvalidStateTransition("Registration has been cancelled.");

        return Result.Success;
    }

    public void SetReadableId(string readableId)
    {
        ReadableId = readableId;
    }

    private readonly List<Attendee> _attendees = new();
    public IReadOnlyCollection<Attendee> Attendees => _attendees.AsReadOnly();

    internal EventRegistration(Guid eventId, Guid registrantId, bool isRegistrantAttending, decimal totalBasePrice, decimal discountAmount = 0, string? appliedPolicies = null) : base(Guid.NewGuid())
    {
        EventId = eventId;
        RegistrantId = registrantId;
        IsRegistrantAttending = isRegistrantAttending;
        TotalBasePrice = totalBasePrice;
        DiscountAmount = discountAmount;
        FinalTotal = totalBasePrice - discountAmount;
        AppliedPolicies = appliedPolicies;
        Status = RegistrationStatus.PendingPayment;
    }

    private EventRegistration() { }

    internal void AddAttendee(Attendee attendee)
    {
        _attendees.Add(attendee);
    }

    public void ApplyPricing(decimal discountAmount, string? appliedPolicies)
    {
        DiscountAmount = discountAmount;
        AppliedPolicies = appliedPolicies;
        FinalTotal = TotalBasePrice - discountAmount;
    }

    public Result<Success> SetInvoiceId(Guid invoiceId)
    {
        if (Status != RegistrationStatus.PendingPayment)
            return EventErrors.NotPendingPayment;

        InvoiceId = invoiceId;
        return Result.Success;
    }

    public Result<Success> MarkAsConfirmed()
    {
        // Must strictly transition from PendingPayment -> Confirmed
        if (Status != RegistrationStatus.PendingPayment)
            return EventErrors.InvalidStateTransition($"Cannot transition to Confirmed from {Status}.");

        Status = RegistrationStatus.Confirmed;
        return Result.Success;
    }

    public Result<Success> Cancel()
    {
        // Can cancel from PendingPayment or Confirmed
        if (Status == RegistrationStatus.Cancelled)
            return EventErrors.InvalidStateTransition("Registration is already cancelled.");

        Status = RegistrationStatus.Cancelled;
        return Result.Success;
    }
}
