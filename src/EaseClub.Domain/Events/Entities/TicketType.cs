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
        int? maxPerMember = null) : base()
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
            return Error.Validation("TicketType.InvalidReserveCount", "Must reserve at least one seat.");
            
        if (AvailableQuantity < count)
            return Error.Validation("TicketType.NotEnoughSeats", $"Not enough '{Name}' tickets available.");

        SoldQuantity += count;
        return Result.Success;
    }

    internal Result<Success> ReleaseSeats(int count)
    {
        if (count <= 0)
            return Error.Validation("TicketType.InvalidReleaseCount", "Must release at least one seat.");

        if (SoldQuantity - count < 0)
            return Error.Validation("TicketType.InvalidRelease", "Cannot release more seats than have been sold.");

        SoldQuantity -= count;
        return Result.Success;
    }
}
