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
    public class PricingPoliciesConfiguration : IEntityTypeConfiguration<PricingPolicy>
    {
        public void Configure(EntityTypeBuilder<PricingPolicy> builder)
        {

            builder.HasKey(p => p.Id);

            // 1. Basic Properties
            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(p => p.Trigger)
                .IsRequired()
                .HasConversion<string>(); // e.g., "OnFieldChanged", "OnSectionAdded"

            builder.Property(p => p.IsPercentage)
                .IsRequired();

            builder.Property(p => p.IsIncrease)
                .IsRequired();

            // 2. Map Money Value Object (EffectAmount)
            builder.OwnsOne(p => p.EffectAmount, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("Effect_Amount")
                    .HasPrecision(18, 2)
                    .IsRequired();

                money.Property(m => m.Currency)
                    .HasColumnName("Effect_Currency")
                    .HasMaxLength(3)
                    .IsRequired();
            });

            // 3. Map ConditionExpression Value Object (Condition)
            // Reusing the flattening strategy we used in Field Definitions
            builder.OwnsOne(p => p.Condition, cond =>
            {
                cond.Property(c => c.DependsOnFieldKey)
                    .HasColumnName("Condition_FieldKey")
                    .HasMaxLength(100)
                    .IsRequired();

                cond.Property(c => c.Operator)
                    .HasColumnName("Condition_Operator")
                    .HasConversion<string>()
                    .IsRequired();

                cond.Property(c => c.ExpectedValue)
                    .HasColumnName("Condition_ExpectedValue")
                    .HasMaxLength(500)
                    .IsRequired();
            });

            // 4. Indexes
            // Since pricing policies are often looked up by trigger
            builder.HasIndex(p => p.Trigger);
        }
    }
}
