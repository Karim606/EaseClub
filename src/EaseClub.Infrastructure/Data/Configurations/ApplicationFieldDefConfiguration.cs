using EaseClub.Domain.ApplicationTemplates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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

            // 3. Map ConditionExpression
            builder.OwnsOne(f => f.VisibilityCondition, nav =>
            {
                nav.ToJson();
            });

            builder.Property(f => f.AllowedValues)
            .HasConversion(
                v => v == null ? null : string.Join(",", v),      // List<string> -> CSV string
                 v => string.IsNullOrEmpty(v)
                    ? null
                    : v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()  // CSV -> List<string>
                )
                .HasColumnType("nvarchar(max)")
                .IsRequired(false)
                .Metadata.SetValueComparer(
                    new ValueComparer<List<string>>(
                        (c1, c2) => c1.SequenceEqual(c2),                          // equality
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())), // hash
                        c => c.ToList()                                              // snapshot
                    )
                );

            builder.HasOne<ApplicationTemplateDefinition>( )  // optional if navigation needed
            .WithMany()
            .HasForeignKey(f => f.TemplateId)
            .OnDelete(DeleteBehavior.Restrict);

            // Optional: Index for faster lookups by Key within a Section
            builder.HasIndex(f => new { f.TemplateId, f.Key }).IsUnique();
            builder.HasIndex(f => new { f.SectionId, f.Order }).IsUnique();



        }
    }
}
