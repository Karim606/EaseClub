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
    public class MembershipTypeConfiguration : IEntityTypeConfiguration<MembershipType>
    {
        public void Configure(EntityTypeBuilder<MembershipType> builder)
        {

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.HasMany(x => x.PermittedBranches)
                   .WithOne(x => x.MembershipType)
                   .HasForeignKey(x => x.MembershipTypeId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(x => x.PermittedBranches).HasField("_PermittedBranches").UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Navigation(x => x.Plans).HasField("_Plans").UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(x => new { x.ClubId, x.IsActive });

            builder.HasIndex(x => new { x.ClubId, x.Name});
        }
    }
}
