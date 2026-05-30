using EaseClub.Domain.Events;
using EaseClub.Domain.Events.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EaseClub.Infrastructure.Auth.Entities;
using EaseClub.Domain.Clubs;

namespace EaseClub.Infrastructure.Data.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).IsRequired().HasMaxLength(2000);
        builder.Property(e => e.StartDate).IsRequired();
        builder.Property(e => e.EndDate).IsRequired();
        builder.Property(e => e.Capacity).IsRequired();
        builder.Property(e => e.AccessType).HasConversion<string>().IsRequired();
        builder.Property(e => e.Status).HasConversion<string>().IsRequired();
        
        builder.Property(e => e.Venue).HasMaxLength(500);
        builder.Property(e => e.Badge).HasMaxLength(100);

        builder.HasOne(e => e.Image)
            .WithMany()
            .HasForeignKey(e => e.ImageId)
            .OnDelete(DeleteBehavior.SetNull);


        // Foreign Key to Club
        builder.HasOne(e => e.Club)
            .WithMany()
            .HasForeignKey(e => e.ClubId)
            .OnDelete(DeleteBehavior.Restrict);

        // TicketTypes - owned collection
        builder.HasMany(e => e.TicketTypes)
            .WithOne()
            .HasForeignKey(t => t.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Event.TicketTypes))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        // Registrations - owned collection
        builder.HasMany(e => e.Registrations)
            .WithOne(r => r.Event)
            .HasForeignKey(r => r.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Event.Registrations))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        // PricingPolicyIds stored as a JSON column
        builder.Property(e => e.PricingPolicyIds)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                       .Select(Guid.Parse)
                       .ToList()
            )
            .Metadata.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class TicketTypeConfiguration : IEntityTypeConfiguration<TicketType>
{
    public void Configure(EntityTypeBuilder<TicketType> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Category).HasConversion<string>().IsRequired();
        builder.Property(t => t.BasePrice).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(t => t.TotalQuantity).IsRequired();
        builder.Property(t => t.SoldQuantity).IsRequired();
        builder.Property(t => t.MaxPerMember);
        builder.Property(t => t.RequiresMembership).IsRequired().HasDefaultValue(false);
        builder.Property(t => t.MinAge);
        builder.Property(t => t.MaxAge);
        builder.Property(t => t.GenderRestriction).HasMaxLength(50);

        builder.Ignore(t => t.AvailableQuantity);
    }
}

public class EventRegistrationConfiguration : IEntityTypeConfiguration<EventRegistration>
{
    public void Configure(EntityTypeBuilder<EventRegistration> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.RegistrantId).IsRequired();
        builder.Property(r => r.IsRegistrantAttending).IsRequired();
        builder.Property(r => r.Status).HasConversion<string>().IsRequired();
        builder.Property(r => r.TotalBasePrice).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(r => r.DiscountAmount).HasColumnType("decimal(18,2)").IsRequired().HasDefaultValue(0);
        builder.Property(r => r.FinalTotal).HasColumnType("decimal(18,2)").IsRequired().HasDefaultValue(0);
        builder.Property(r => r.AppliedPolicies).HasMaxLength(1000);
        builder.Property(r => r.InvoiceId);

        // Foreign Key to AuthUser (Registrant)
        builder.HasOne<AuthUser>()
            .WithMany()
            .HasForeignKey(r => r.RegistrantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.Attendees)
            .WithOne()
            .HasForeignKey(a => a.EventRegistrationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(EventRegistration.Attendees))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class AttendeeConfiguration : IEntityTypeConfiguration<Attendee>
{
    public void Configure(EntityTypeBuilder<Attendee> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.TicketTypeId).IsRequired();
        builder.Property(a => a.AttendeeId);
        builder.Property(a => a.AttendeeName).HasMaxLength(200);
        builder.Property(a => a.Age);
        builder.Property(a => a.Gender).HasMaxLength(50);

        // Foreign Key to AuthUser (Attendee - nullable)
        builder.HasOne<AuthUser>()
            .WithMany()
            .HasForeignKey(a => a.AttendeeId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
