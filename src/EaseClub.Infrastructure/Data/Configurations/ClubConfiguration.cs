using EaseClub.Domain.Branches;
using EaseClub.Domain.Clubs;
using EaseClub.Domain.Clubs.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ChangeTracking;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Data.Configurations
{
    public class ClubConfiguration : IEntityTypeConfiguration<Club>
    {
        public void Configure(EntityTypeBuilder<Club> builder)
        {
            builder.HasKey(c => c.Id);

            builder.HasOne(c => c.Logo)
                   .WithMany()
                   .HasForeignKey(c => c.LogoId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(c => c.CoverImage)
                   .WithMany()
                   .HasForeignKey(c => c.CoverImageId)
                   .OnDelete(DeleteBehavior.NoAction);

                  builder
                 .HasMany(c => c.Branches)
                 .WithOne(b => b.Club)
                 .HasForeignKey(b => b.ClubId)
                 .OnDelete(DeleteBehavior.NoAction);

            builder.Navigation(c => c.Branches).HasField("_Branches").UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(c => c.ClubAdmins)
                   .WithOne(ca => ca.Club)
                   .HasForeignKey(ca => ca. ClubId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.Code)
            .IsUnique();

            builder.Navigation(c => c.ClubAdmins).HasField("_ClubAdmins").UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.Navigation(c => c.Memberships).HasField("_Memberships").UsePropertyAccessMode(PropertyAccessMode.Field);
             
            builder.Navigation(c => c.WorkSchedules).HasField("_workSchedules").UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.Navigation(c => c.Amenities).HasField("_amenities").UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.OwnsOne(c => c.ContactInfo, ci =>
            {
                ci.OwnsOne(c => c.Email, e =>
                {
                    e.Property(p => p.Value)
                        .HasColumnName("ContactEmail")
                        .IsRequired();
                });

                ci.OwnsOne(c => c.Phone, p =>
                {
                    p.Property(x => x.Value)
                        .HasColumnName("ContactPhone");
                });
            });

            builder.Property(c => c.Amenities)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<List<Amenity>>(v, (JsonSerializerOptions)null) ?? new List<Amenity>()
            )
            .Metadata.SetValueComparer(new ValueComparer<IReadOnlyCollection<Amenity>>(
                (c1, c2) => c1!.SequenceEqual(c2!),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList()));

            builder.Property(c => c.WorkSchedules)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<List<WorkSchedule>>(v, (JsonSerializerOptions)null) ?? new List<WorkSchedule>()
                )
                .HasColumnType("nvarchar(max)")
                .Metadata.SetValueComparer(new ValueComparer<IReadOnlyCollection<WorkSchedule>>(
                (c1, c2) => c1!.SequenceEqual(c2!),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList()));
        }
    }
}
