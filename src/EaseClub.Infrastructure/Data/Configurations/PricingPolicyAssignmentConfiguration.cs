using EaseClub.Domain.PricingPolices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Data.Configurations
{
    public class PricingPolicyAssignmentConfiguration : IEntityTypeConfiguration<PricingPolicyAssignment>
    {
        public void Configure(EntityTypeBuilder<PricingPolicyAssignment> builder)
        {
            builder.ToTable("PricingPolicyAssignments");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.PolicyId).IsRequired();
            builder.Property(a => a.TargetId).IsRequired();
            builder.Property(a => a.Priority).IsRequired();

            builder.Property(a => a.TargetType)
                .HasConversion<string>() // store enum as string
                .IsRequired();

            // Optional: unique index to prevent duplicate policy assignment for same template
            builder.HasIndex(a => new { a.TargetId, a.PolicyId, a.TargetType })
                .IsUnique();
        }
    }
}
