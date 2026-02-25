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
    public class ApplicationStepDefConfiguration:IEntityTypeConfiguration<ApplicationStepDefinition>
    {
        public void Configure(EntityTypeBuilder<ApplicationStepDefinition> builder)
        {

            builder.HasKey(s => s.Id);

            // 1. Basic Properties
            builder.Property(s => s.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(s => s.Category)
                .IsRequired()
                .HasMaxLength(50); // e.g., "IDENTITY", "DOCUMENTS"

            builder.Property(s => s.Order)
                .IsRequired();



            // Children: Sections
            // Configure the private backing field for encapsulated collection
            builder.Navigation(x => x.Sections).HasField("_Sections")
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(s => s.Sections)
                .WithOne(sec => sec.Step)
                .HasForeignKey(sec => sec.StepId)
                .OnDelete(DeleteBehavior.Cascade);

            // 3. Indexes for performance
            // Speeds up fetching steps for a specific template in order
            builder.HasIndex(s => new { s.TemplateId, s.Order });
        }
    }
}
