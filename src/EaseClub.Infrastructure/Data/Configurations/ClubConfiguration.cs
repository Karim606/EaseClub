using EaseClub.Domain.Branches;
using EaseClub.Domain.Clubs;

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
    public class ClubConfiguration : IEntityTypeConfiguration<Club>
    {
        public void Configure(EntityTypeBuilder<Club> builder)
        {
            builder.HasKey(c => c.Id);

                  builder
                 .HasMany(c => c.Branches)
                 .WithOne(b => b.Club)
                 .HasForeignKey(b => b.ClubId)
                 .OnDelete(DeleteBehavior.NoAction);

            builder.Navigation(c => c.Branches).HasField("_Branches").UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
