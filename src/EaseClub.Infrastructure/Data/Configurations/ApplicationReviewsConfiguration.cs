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
    public class ApplicationReviewsConfiguration: IEntityTypeConfiguration<ApplicationReview>
    {
        public void Configure(EntityTypeBuilder<ApplicationReview> builder)
        {
            builder.ToTable("ApplicationReviews");

            // Primary Key
            builder.HasKey(r => r.Id);

            // 1. Basic Properties
            builder.Property(r => r.Decision)
                .IsRequired()
                .HasConversion<string>(); // Stores 'Approved', 'Reject', etc. as readable strings

            builder.Property(r => r.Reason)
                .HasMaxLength(1000) // Reviews might need long explanations
                .IsRequired(); // Since the factory handles empty as string.Empty

            builder.Property(r => r.Date)
                .IsRequired();

            // 2. Foreign Keys
            builder.Property(r => r.ApplicationId)
                .IsRequired();

            builder.Property(r => r.ReviewerId)
                .IsRequired();

            // 3. Relationships
            // Linked to the Application. Usually, one application has many reviews (history)
            builder.HasOne<MembershipApplication>()
                .WithMany(a => a.Reviews) // You could add a 'Reviews' collection to MembershipApplication if needed
                .HasForeignKey(r => r.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            // 4. Indexes
            builder.HasIndex(r => r.ApplicationId);
            builder.HasIndex(r => r.ReviewerId);
        }
    }
}
