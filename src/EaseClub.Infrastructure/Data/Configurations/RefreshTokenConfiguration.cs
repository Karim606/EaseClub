using EaseClub.Infrastructure.Auth.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Data.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.Token).IsUnique();

            builder.Property(x => x.Token).IsRequired();
            builder.Property(x => x.ExpiresAt).IsRequired();
            builder.Property(x => x.CreatedAt).IsRequired();
            

            // Relationship: AuthUser 1..* RefreshTokens
            builder.HasOne(x => x.User)
                   .WithMany(u => u.RefreshTokens) // points to public collection
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Cascade); // delete tokens if user is deleted
        }
    }
}
