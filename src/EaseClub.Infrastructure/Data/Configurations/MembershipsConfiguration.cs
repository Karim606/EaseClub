using EaseClub.Domain.Clubs;
using EaseClub.Domain.Common.Interfaces;
using EaseClub.Domain.Memberships;
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
    public class MembershipsConfiguration : IEntityTypeConfiguration<Membership>
    {
        public void Configure(EntityTypeBuilder<Membership> builder)
        {
            builder.HasKey(m => m.Id);
            builder.HasIndex(m => new { m.MembershipTypeId, m.ClubId, m.UserId }).IsUnique();

            builder.HasOne(m => m.MembershipType).WithMany().HasForeignKey(m => m.MembershipTypeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<Club>().WithMany().HasForeignKey(m => m.ClubId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(m => m.MembershipPlan).WithMany().HasForeignKey(m => m.MembershipPlanId).OnDelete(DeleteBehavior.Restrict);

            builder.Property(m => m.Status).HasConversion<string>(); // Enum

            
                builder.OwnsOne(m => m.Period, period =>
                {
                    period.Property(p => p.StartDate).IsRequired();
                    period.Property(p => p.EndDate).IsRequired();
                });

        }
    }
}
