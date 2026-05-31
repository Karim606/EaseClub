using System;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Events.Enums;

namespace EaseClub.Domain.Events.Entities;

public class TicketType : Entity
{
    public Guid EventId { get; private set; }
    public AttendeeCategory Category { get; private set; }
    public decimal BasePrice { get; private set; }
    
    public int TotalQuantity { get; private set; }
    public int SoldQuantity { get; private set; }
    public int AvailableQuantity => TotalQuantity - SoldQuantity;
    
    public int? MaxPerMember { get; private set; }
    public bool RequiresMembership { get; private set; }
    public int? MinAge { get; private set; }
    public int? MaxAge { get; private set; }
    public string? GenderRestriction { get; private set; }

    internal TicketType(
        Guid eventId, 
        AttendeeCategory category, 
        decimal basePrice, 
        int totalQuantity, 
        int? maxPerMember = null,
        int? minAge = null,
        int? maxAge = null,
        string? genderRestriction = null) : base(Guid.NewGuid())
    {
        EventId = eventId;
        Category = category;
        BasePrice = basePrice;
        TotalQuantity = totalQuantity;
        SoldQuantity = 0;
        MaxPerMember = maxPerMember;
        RequiresMembership = category == AttendeeCategory.Member || category == AttendeeCategory.FamilyMember;
        MinAge = minAge;
        MaxAge = maxAge;
        GenderRestriction = genderRestriction;
    }

    private TicketType() { }

    internal Result<Success> UpdateDetails(
        AttendeeCategory category,
        decimal basePrice, 
        int totalQuantity, 
        int? maxPerMember,
        int? minAge,
        int? maxAge,
        string? genderRestriction)
    {
        if (basePrice < 0)
            return EventErrors.InvalidTicketPrice;

        if (totalQuantity < SoldQuantity)
            return EventErrors.InvalidTicketQuantity;

        Category = category;
        BasePrice = basePrice;
        TotalQuantity = totalQuantity;
        MaxPerMember = maxPerMember;
        RequiresMembership = category == AttendeeCategory.Member || category == AttendeeCategory.FamilyMember;
        MinAge = minAge;
        MaxAge = maxAge;
        GenderRestriction = genderRestriction;
        
        return Result.Success;
    }

    internal Result<Success> ReserveSeats(int count)
    {
        if (count <= 0)
            return EventErrors.InvalidReserveCount;
            
        if (AvailableQuantity < count)
            return EventErrors.NotEnoughSeats(Category.ToString());

        SoldQuantity += count;
        return Result.Success;
    }

    internal Result<Success> ReleaseSeats(int count)
    {
        if (count <= 0)
            return EventErrors.InvalidReleaseCount;

        if (SoldQuantity - count < 0)
            return EventErrors.InvalidRelease;

        SoldQuantity -= count;
        return Result.Success;
    }
}
