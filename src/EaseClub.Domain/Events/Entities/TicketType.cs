using System;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Events.Enums;

namespace EaseClub.Domain.Events.Entities;

public class TicketType : Entity
{
    public Guid EventId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public AttendeeCategory Category { get; private set; }
    public decimal Price { get; private set; }
    
    public int Quantity { get; private set; }
    public int SoldQuantity { get; private set; }
    public int AvailableQuantity => Quantity - SoldQuantity;
    
    public int? MaxPerMember { get; private set; }

    internal TicketType(
        Guid eventId, 
        string name, 
        string description, 
        AttendeeCategory category, 
        decimal price, 
        int quantity, 
        int? maxPerMember = null) : base(Guid.NewGuid())
    {
        EventId = eventId;
        Name = name;
        Description = description;
        Category = category;
        Price = price;
        Quantity = quantity;
        SoldQuantity = 0;
        MaxPerMember = maxPerMember;
    }

    // Default constructor for EF Core
    private TicketType() { }

    internal void UpdateDetails(
        string name, 
        string description, 
        decimal price, 
        int quantity, 
        int? maxPerMember)
    {
        Name = name;
        Description = description;
        Price = price;
        Quantity = quantity;
        MaxPerMember = maxPerMember;
    }

    internal Result<Success> ReserveSeats(int count)
    {
        if (count <= 0)
            return EventErrors.InvalidReserveCount;
            
        if (AvailableQuantity < count)
            return EventErrors.NotEnoughSeats(Name);

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
