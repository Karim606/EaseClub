using EaseClub.Domain.Common;
using EaseClub.Domain.Common.ValueObjects;
using EaseClub.Domain.Member;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Data.Configurations
{
    public class UserBaseConfiguration : IEntityTypeConfiguration<UserBase>
    {
        public void Configure(EntityTypeBuilder<UserBase> builder)
        {
            builder.HasKey(u => u.Id);
            builder.Property(u => u.FirstName)
                   .IsRequired()
                   .HasMaxLength(100);
            builder.Property(u => u.LastName)
                   .IsRequired()
                   .HasMaxLength(100);

            

            builder.OwnsOne(m => m.Email, email =>
            {
                email.Property(e => e.Value)
                .HasColumnName("Email")   // Column name in table
                .HasMaxLength(256)
                .IsRequired();

                email.HasIndex(e => e.Value)
                   .IsUnique()
                   .IsClustered(false);
            });

            builder.OwnsOne(m => m.PhoneNumber, PhoneNumber =>
            {
                PhoneNumber.Property(p => p.Value)
                .HasColumnName("PhoneNumber")   // Column name in table
                .HasMaxLength(20)
                .IsRequired();

                PhoneNumber.HasIndex(p => p.Value)
                   .IsUnique()
                   .IsClustered(false);
            });
        }
    }
}
