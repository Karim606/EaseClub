using EaseClub.Domain.ApplicationTemplates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Data.Configurations
{
    public class ApplicationSectionDefConfiguration : IEntityTypeConfiguration<ApplicationSectionDefinition>
    {
        public void Configure(EntityTypeBuilder<ApplicationSectionDefinition> builder)
        {

            builder.HasKey(s => s.Id);
            builder.HasIndex(sec => new { sec.StepId, sec.Order })
           .IsUnique();

            // 1. Basic Properties
            builder.Property(s => s.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(s => s.Order)
                .IsRequired();

            // 2. Map RepeatRule (Value Object)
            // We use OwnsOne to flatten it into the Section table
            builder.OwnsOne(s => s.RepeatRule, nav =>
            {
                nav.Property(r => r.NumberOfRepeats)
                    .HasColumnName("NumberOfRepeats")
                    .HasColumnType("int") // Explicitly tells the DB to use an integer type
                    .IsRequired();

                nav.Property(r => r.Mode)
                    .HasColumnName("Repeat_Mode")
                    .HasConversion<string>() // Stores 'ExactValue', 'AtLeastOne', etc.
                    .IsRequired();
            });

            // 3. Relationships

            // Children: Fields
            // We tell EF to use the private backing field for the collection
            builder.Navigation(x=> x.Fields).HasField("_Fields")
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(s => s.Fields)
                .WithOne(f => f.Section) // ApplicationFieldDefinition has SectionId but doesn't necessarily need a navigation property back to Section
                .HasForeignKey(f => f.SectionId)
                .OnDelete(DeleteBehavior.ClientCascade);

            // 4. Ignore read-only domain properties
            builder.Ignore(s => s.IsRepeatable);

            
        }
    }
}
