using EaseClub.Domain.ApplicationTemplates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using EaseClub.Domain.MembershipApplications.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Data.Configurations
{
    public class ApplicationTemplateDefConfiguration : IEntityTypeConfiguration<ApplicationTemplateDefinition>
    {
        private JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter() }
        };

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

            builder.Property(t => t.Steps)
                .HasColumnType("nvarchar(max)")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<List<StepSnapshot>>(v, jsonOptions) ?? new List<StepSnapshot>()
                )
                .Metadata.SetValueComparer(new ValueComparer<IReadOnlyList<StepSnapshot>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));

            builder.Navigation(x => x.ConnectedMembershipPlans).HasField("_ConnectedMembershipPlans")
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.Navigation(x => x.PricingPolicyAssignments).HasField("_PricingPolicyAssignments")
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(t => t.ConnectedMembershipPlans)
                .WithOne()
                .HasForeignKey(mp => mp.ApplicationTemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            // 4. Indexes
            // Index for club lookups (Common query)
            builder.HasIndex(t => t.ClubId);
        }
    }
}
