using EaseClub.Domain.MembershipPlans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Data.Configurations
{
    public class MembershipInstallmentConfiguration : IEntityTypeConfiguration<MembershipInstallment>
    {
        public void Configure(EntityTypeBuilder<MembershipInstallment> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Amount).HasPrecision(18, 2).IsRequired();

            // Enum conversion to string or int
            builder.Property(x => x.Status)
                   .HasConversion<string>()
                   .HasMaxLength(20);

            builder.HasIndex(x => x.MembershipCycleId);

            builder.HasOne(x=>x.Club).WithMany().HasForeignKey(x => x.ClubId).OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.DueDate)
            .IsRequired();

            builder.HasIndex(x => new { x.MembershipCycleId, x.Order })
            .IsUnique();

            builder.HasIndex(x => x.DueDate);
        }
    }
}
