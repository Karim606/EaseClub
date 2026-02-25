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
    public class ApplicationFieldDefConfiguration : IEntityTypeConfiguration<ApplicationFieldDefinition>
    {
        public void Configure(EntityTypeBuilder<ApplicationFieldDefinition> builder)
        {
            builder.ToTable("ApplicationFieldDefinitions");

            builder.HasKey(f => f.Id);

            // 1. Basic Properties
            builder.Property(f => f.Key)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(f => f.Type)
                .IsRequired()
                .HasConversion<string>(); // Stores Enum as String (e.g., "Text", "Number")

            builder.Property(f => f.Order)
                .IsRequired();

            // 2. Map ValidationRuleSet (Stored as a JSON Column)
            // This allows the flexible Rules collection to live in one column
            builder.OwnsOne(f => f.ValidationRules, nav =>
            {
                nav.ToJson();
            });

            // 3. Map ConditionExpression (Flattened into the same table)
            builder.OwnsOne(f => f.VisibilityCondition, nav =>
            {
                // Renaming columns to be descriptive in the table
                nav.Property(c => c.DependsOnFieldKey)
                    .HasColumnName("Visibility_DependsOnFieldKey")
                    .HasMaxLength(100);

                nav.Property(c => c.Operator)
                    .HasColumnName("Visibility_Operator")
                    .HasConversion<string>();

                nav.Property(c => c.ExpectedValue)
                    .HasColumnName("Visibility_ExpectedValue")
                    .HasMaxLength(500);
            });




            // Optional: Index for faster lookups by Key within a Section
            builder.HasIndex(f => new { f.SectionId, f.Key }).IsUnique();
            builder.HasIndex(f => new { f.SectionId, f.Order }).IsUnique();
            builder.HasMany(f => f.PricingPolices).WithMany();
            builder.Navigation(f => f.PricingPolices).HasField("_PricingPolicies").UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
