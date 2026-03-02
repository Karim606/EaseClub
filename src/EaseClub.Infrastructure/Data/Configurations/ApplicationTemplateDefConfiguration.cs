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
    public class ApplicationTemplateDefConfiguration : IEntityTypeConfiguration<ApplicationTemplateDefinition>
    {
        public void Configure(EntityTypeBuilder<ApplicationTemplateDefinition> builder)
        {

            // Primary Key
            builder.HasKey(t => t.Id);

            // 1. Basic Properties
            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(250);


            builder.Property(t => t.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(t => t.ClubId)
                .IsRequired();


            // 3. Relationships (The Step Collection)
            // Access the private backing field _Steps
            builder.Navigation(x=> x.Steps).HasField("_Steps")
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            //builder.Navigation(x => x.Fields).HasField("_Fields")
            //    .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(t => t.Steps)
                .WithOne(s => s.Template)
                .HasForeignKey(s => s.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            // 4. Indexes
            // Index for club lookups (Common query)
            builder.HasIndex(t => t.ClubId);

            // Optional: Ensure only one 'Active' template per name per club
            // builder.HasIndex(t => new { t.ClubId, t.Name, t.IsActive })
            //    .HasFilter("[IsActive] = 1");
        }
    }
}
