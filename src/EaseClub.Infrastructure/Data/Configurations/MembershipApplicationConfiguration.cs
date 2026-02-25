using EaseClub.Domain.MembershipApplications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Data.Configurations
{
    public class MembershipApplicationConfiguration:IEntityTypeConfiguration<MembershipApplication>
    {
        public void Configure(EntityTypeBuilder<MembershipApplication> builder)
        {
            builder.ToTable("MembershipApplications");

            // Primary Key
            builder.HasKey(a => a.Id);

            // 1. Core Properties
            builder.Property(a => a.TrackingNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(a => a.TemplateSnapshot)
                .IsRequired()
                .HasColumnType("nvarchar(max)"); // Ensure enough space for large JSON snapshots

            builder.Property(a => a.Status)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(a => a.PricingState)
                .IsRequired()
                .HasConversion<string>();

            // 2. Financials
            builder.Property(a => a.BasePrice)
                .HasPrecision(18, 2);

            builder.Property(a => a.FinalPrice)
                .HasPrecision(18, 2);

            // 3. Relationships (Answers Collection)
            // Access the private backing field _Answers for encapsulation
            builder.Navigation(x => x.Answers).HasField("_Answers")
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(a => a.Answers)
                .WithOne() // ApplicationAnswer can exist without a navigation back to Application
                .HasForeignKey(ans => ans.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            // 4. Indexes
            builder.HasIndex(a => a.TrackingNumber).IsUnique();
            builder.HasIndex(a => a.UserId);
            builder.HasIndex(a => a.ClubId);
            builder.HasIndex(a => a.Status);
        }
    }
}
