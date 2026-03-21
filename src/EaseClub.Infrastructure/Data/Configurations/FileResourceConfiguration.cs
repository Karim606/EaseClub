using EaseClub.Domain.Files;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Data.Configurations
{
    public class FileResourceConfiguration : IEntityTypeConfiguration<FileResource>
    {
        public void Configure(EntityTypeBuilder<FileResource> builder)
        {
            builder.ToTable("FileResources");


            // Primary Key
            builder.HasKey(f => f.Id);

            // Properties
            builder.Property(f => f.FileName)
                   .IsRequired()
                   .HasMaxLength(255);

            builder.Property(f => f.FilePath)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(f => f.ContentType)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(f => f.Size)
                   .IsRequired();

            builder.Property(f => f.IsPrivate)
                   .IsRequired();

            builder.Property(f => f.IsTemporary)
                   .IsRequired();

            builder.Property(f => f.Category)
                   .IsRequired();

            // Relationships

            // Club (optional)
            builder.HasOne(f => f.Club)
                   .WithMany( )   // assume Club has ICollection<FileResource> Files
                   .HasForeignKey(f => f.ClubId)
                   .OnDelete(DeleteBehavior.SetNull); // keep file if club is deleted

            // Membership Application (optional)
            builder.HasOne(f => f.Application)
                   .WithMany( )   // assume MembershipApplication has ICollection<FileResource> Files
                   .HasForeignKey(f => f.ApplicationId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
