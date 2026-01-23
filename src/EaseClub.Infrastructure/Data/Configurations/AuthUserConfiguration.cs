using EaseClub.Infrastructure.Auth.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EaseClub.Domain.Common;

namespace EaseClub.Infrastructure.Data.Configurations
{
    public class AuthUserConfiguration : IEntityTypeConfiguration<AuthUser>
    {
        public void Configure(EntityTypeBuilder<AuthUser> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.Email).IsUnique();
            builder.Property(x => x.Email).IsRequired();
            builder.HasOne(x => x.DomainUserBase).WithOne().HasForeignKey<AuthUser>(x => x.Id).OnDelete(DeleteBehavior.Cascade);

        }
    }
}
