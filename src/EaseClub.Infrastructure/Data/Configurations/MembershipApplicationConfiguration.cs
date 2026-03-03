using EaseClub.Domain.MembershipApplications;
using EaseClub.Domain.MembershipApplications.ValueObjects;
using EaseClub.Domain.PricingPolices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Data.Configurations
{
    public class MembershipApplicationConfiguration:IEntityTypeConfiguration<MembershipApplication>
    {
        private JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter() }
        };

        public void Configure(EntityTypeBuilder<MembershipApplication> builder)
        {
            builder.ToTable("MembershipApplications");

            // Primary Key
            builder.HasKey(a => a.Id);

            // 1. Core Properties
            builder.Property(a => a.TrackingNumber)
                .IsRequired()
                .HasMaxLength(50);


            builder.Property(a => a.Status)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(a => a.PricingState)
                .IsRequired()
                .HasConversion<string>();

            //builder.OwnsOne(a => a.TemplateSnapshot, snapshot =>
            //{
            //    snapshot.ToJson();

            //    snapshot.OwnsMany(s => s.Policies)
            //    .OwnsMany(s => s.Conditions);

            //    snapshot.OwnsMany(s => s.Steps, step =>
            //    {
            //        step.OwnsMany(s => s.Sections, section =>
            //        {
            //            section.OwnsMany(s => s.Fields, field =>
            //            {
            //                field.OwnsOne(f => f.ValidationRules);
            //                field.OwnsOne(f => f.VisibilityCondition);
            //                field.Property(f => f.Type)
            //                .HasConversion<string>();
            //            });
            //            section.OwnsOne(s => s.RepeatRule);
            //        });
            //    });
            //});

            //builder.OwnsOne(a => a.FinalPriceSummary, price =>
            //{
            //    price.Property(p => p.TotalPrice).HasPrecision(18, 2);
            //    price.Property(p => p.BasePrice).HasPrecision(18, 2);
            //    price.OwnsMany(p => p.AppliedPolicies);

            //    price.ToJson();

            //});
            builder.Property(a => a.TemplateSnapshot)
            .HasColumnType("nvarchar(max)")
            .HasConversion(
                v => JsonSerializer.Serialize(v, jsonOptions),
                v => JsonSerializer.Deserialize<ApplicationTemplateSnapshot>(v, jsonOptions)!
            );

            builder.Property(a => a.FinalPriceSummary)
                .HasColumnType("nvarchar(max)")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<PricingResult>(v, jsonOptions)
                );

            // Handle the private List<int> field
            builder.Property(a => a.CompletedStepOrders)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<List<int>>(v, jsonOptions) ?? new List<int>()
                );

            // 3. Relationships (Answers Collection)
            // Access the private backing field _Answers for encapsulation
            builder.Navigation(x => x.Answers).HasField("_Answers")
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.Property(a => a.CompletedStepOrders)
            .HasField("_CompletedStepOrders")
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
