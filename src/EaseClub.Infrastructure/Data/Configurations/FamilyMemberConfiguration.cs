using EaseClub.Domain.Memberships;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Data.Configurations
{
    public class FamilyMemberConfiguration : IEntityTypeConfiguration<FamilyMember>
    {
        public void Configure(EntityTypeBuilder<FamilyMember> builder)
        {
            builder.ToTable("FamilyMembers");

            builder.HasKey(f => f.Id);

            // Required Properties
            builder.Property(f => f.FullName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(f => f.Relationship)
                .HasConversion<string>(); // Maps Enum to string in DB

            builder.Property(f => f.DateOfBirth)
                .HasColumnType("date"); // Maps to SQL DATE type

            // Mapping the private dictionary to a JSON column
            builder.Property(f => f.AdditionalData)
                .HasColumnName("AdditionalData")
                .HasColumnType("nvarchar(max)") 
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions)null!)
                         ?? new Dictionary<string, string>()
                );

        }
    }
}
