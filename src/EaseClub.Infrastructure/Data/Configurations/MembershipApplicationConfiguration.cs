using EaseClub.Domain.MembershipApplications;
using EaseClub.Domain.MembershipApplications.ValueObjects;
using EaseClub.Domain.PricingPolices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
            // 2. JSON Blobs (Steps & Progress)
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

            // Progress tracking (Private Field + JSON Conversion + Comparer)
            builder.Property(a => a.CompletedStepOrders)
                .HasField("_CompletedStepOrders")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<List<int>>(v, jsonOptions) ?? new List<int>()
                )
                .Metadata.SetValueComparer(new ValueComparer<IReadOnlyList<int>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));

            // Answers collection (Private Field + JSON Conversion + Comparer)
            builder.Property(a => a.Answers)
                .HasField("_Answers")
                .HasColumnType("nvarchar(max)")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<List<UserAnswer>>(v, jsonOptions) ?? new List<UserAnswer>()
                )
                .Metadata.SetValueComparer(new ValueComparer<IReadOnlyList<UserAnswer>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));

            // Reviews (Private Field)
            builder.Navigation(x => x.Reviews)
                .HasField("_Reviews")
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasOne(a => a.MembershipType).WithMany().HasForeignKey(a=> a.MembershipTypeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(a => a.MembershipPlan).WithMany().HasForeignKey(a => a.MembershipPlanId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(a =>a.Member).WithMany().HasForeignKey(a => a.MemberId).OnDelete(DeleteBehavior.Restrict);
             builder.HasOne(a => a.Club).WithMany().HasForeignKey(a => a.ClubId).OnDelete(DeleteBehavior.Restrict);
            // 4. Indexes
            builder.HasIndex(a => a.TrackingNumber).IsUnique();
            builder.HasIndex(a => a.MemberId);
            builder.HasIndex(a => a.ClubId);
            builder.HasIndex(a => a.Status);
        }
    }
}
