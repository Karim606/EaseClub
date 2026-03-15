using EaseClub.Domain.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Data.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notifications");

            builder.HasKey(n => n.Id);

            builder.Property(n => n.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(n => n.Message)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(n => n.Type)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(n => n.CreatedAt)
                .IsRequired();

            builder.Property(n => n.UserId)
                .IsRequired(false);

            builder.Property(n => n.ClubId)
                .IsRequired(false);

            // Indexes for fast querying
            builder.HasIndex(n => n.UserId);
            builder.HasIndex(n => n.ClubId);
            builder.HasIndex(n => n.CreatedAt);

            // Optional: Prevent both UserId and ClubId being null
            builder.HasCheckConstraint(
                "CK_Notification_Target",
                "[UserId] IS NOT NULL OR [ClubId] IS NOT NULL"
            );
        }
    }
}
