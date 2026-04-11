using EaseClub.Domain.ApplicationTemplates;
using EaseClub.Domain.Memberships;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Data.Configurations
{
    public class MembershipCycleConfiguration : IEntityTypeConfiguration<MembershipCycle>
    {
        public void Configure(EntityTypeBuilder<MembershipCycle> builder)
        {
            builder.HasKey(x => x.Id);

            builder.OwnsOne(m => m.Period, period =>
            {
                period.Property(p => p.StartDate).HasColumnName("StartDate");
                period.Property(p => p.EndDate).HasColumnName("EndDate");

            });



            builder.HasMany(x => x.Installments)
            .WithOne()
            .HasForeignKey(x => x.MembershipCycleId)
            .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Membership)
                .WithMany(m => m.MembershipCycles)
                .HasForeignKey(x => x.MembershipId)
                .OnDelete(DeleteBehavior.Restrict);


            builder.Navigation(x => x.Installments).HasField("_Installments").UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
