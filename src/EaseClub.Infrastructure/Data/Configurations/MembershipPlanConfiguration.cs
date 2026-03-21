using EaseClub.Domain.MembershipPlans;
using EaseClub.Domain.MembershipTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Data.Configurations
{
    public class MembershipPlanConfiguration : IEntityTypeConfiguration<MembershipPlan>
    {
        public void Configure(EntityTypeBuilder<MembershipPlan> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => new { x.ClubId, x.Name })
            .IsUnique();

            builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.Property(x => x.TotalPrice).HasPrecision(18, 2);

            // Relationships
            builder.HasMany(x => x.InstallmentTemplates)
                   .WithOne(x => x.MembershipPlan)
                   .HasForeignKey(x => x.MembershipPlanId);

            builder.HasOne(p => p.MembershipType) // Plan has one Type
               .WithMany(mt => mt.Plans)  // Type has many Plans
               .HasForeignKey(p => p.MembershipTypeId)
               .OnDelete(DeleteBehavior.Restrict); // Prevent deleting a type if plans exist


            builder.Navigation(x => x.InstallmentTemplates).HasField("_InstallmentTemplates").UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
