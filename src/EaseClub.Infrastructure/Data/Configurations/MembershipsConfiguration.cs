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
            builder.HasIndex(m => new { m.MembershipTypeId, m.ClubId, m.MemberId }).IsUnique();

            builder.HasOne(m => m.MembershipType).WithMany().HasForeignKey(m => m.MembershipTypeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(m => m.Club).WithMany(c => c.Memberships).HasForeignKey(m => m.ClubId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(m => m.MembershipPlan).WithMany(p=>p.Memberships).HasForeignKey(m => m.MembershipPlanId).OnDelete(DeleteBehavior.Restrict);

            builder.Property(m => m.Status).HasConversion<string>(); // Enum

            builder.Navigation(x => x.MembershipCycles).HasField("_MembershipCycles").UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.HasMany(x => x.FamilyMembers).WithOne(x => x.Membership).HasForeignKey(x => x.MembershipId);
            builder.Navigation(x => x.FamilyMembers).HasField("_FamilyMembers").UsePropertyAccessMode(PropertyAccessMode.Field);


            builder.Property(x => x.MembershipNumber)
            .IsRequired()
            .HasMaxLength(30);

            builder.HasIndex(x => x.MembershipNumber)
                .IsUnique();
        }
    }
}
