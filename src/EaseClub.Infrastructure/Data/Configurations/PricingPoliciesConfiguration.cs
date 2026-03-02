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
    public class PricingPoliciesConfiguration : IEntityTypeConfiguration<PricingPolicy>
    {
        public void Configure(EntityTypeBuilder<PricingPolicy> builder)
        {

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

            builder.Property(p => p.FixedAmount)
                .HasPrecision(18, 2);

            builder.Property(p => p.PercentageValue)
                .HasPrecision(18, 2); // e.g., 15.50%

            builder.Property(p => p.MultiplierSourceKey)
                .HasMaxLength(100);

            // ============================================================
            // Mapping the Conditions List as JSON
            // ============================================================
            // We use ToJson() so we don't need a separate join table for conditions.
            // This keeps performance high and the schema simple.
            builder.OwnsMany(p => p.Conditions, builder =>
            {
                builder.ToJson();
            });

            // Ensure the backing field is used for the read-only property
            builder.Navigation(p => p.Conditions)
                .HasField("_Conditions")
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.Property(p => p.ClubId).IsRequired();
        }
    }
}
