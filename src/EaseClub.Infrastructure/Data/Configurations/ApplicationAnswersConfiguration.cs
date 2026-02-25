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
    public class ApplicationAnswersConfiguration : IEntityTypeConfiguration<ApplicationAnswer>
    {
        public void Configure(EntityTypeBuilder<ApplicationAnswer> builder)
        {
            builder.ToTable("ApplicationAnswers");

            // Primary Key
            builder.HasKey(a => a.Id);

            // 1. Core Properties
            builder.Property(a => a.FieldKey)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(a => a.Value)
                .IsRequired(false) // Allow null/empty if a field is cleared
                .HasColumnType("nvarchar(max)"); // Support long text or serialized sub-data

            builder.Property(a => a.InstanceIndex)
                .IsRequired()
                .HasDefaultValue(0);

            // 2. Foreign Keys
            builder.Property(a => a.ApplicationId)
                .IsRequired();

            builder.Property(a => a.FieldDefinitionId)
                .IsRequired();

            // 3. Relationships
            // Linked to the Aggregate Root (MembershipApplication)
            builder.HasOne<MembershipApplication>()
                .WithMany(app => app.Answers)
                .HasForeignKey(a => a.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            // 4. Critical Performance Indexes
            // Index for quickly retrieving all answers for a single application
            builder.HasIndex(a => a.ApplicationId);

            // Unique constraint: One answer per field per instance in an application
            // This prevents duplicate data entries for the same question.
            builder.HasIndex(a => new { a.ApplicationId, a.FieldDefinitionId, a.InstanceIndex })
                .IsUnique();
        }
    }
}
