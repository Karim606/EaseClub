using EaseClub.Infrastructure.Notifications.UserDevices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Data.Configurations
{
    public class UserDeviceConfiguration : IEntityTypeConfiguration<UserDevice>
    {
        public void Configure(EntityTypeBuilder<UserDevice> builder)
        {
            builder.ToTable("UserDevices");

            builder.HasKey(x => x.Id);

            // 1. Unique Constraint on Token
            builder.HasIndex(x => x.FcmToken)
                   .IsUnique();

            // 2. Unique Constraint on DeviceId (The Anchor)
            builder.HasIndex(x => x.DeviceId)
                   .IsUnique();

            // 3. Performance Index for SendToUserAsync
            builder.HasIndex(x => x.UserId);

            // 4. Property Configurations
            builder.Property(x => x.FcmToken)
                   .IsRequired()
                   .HasMaxLength(500); // FCM tokens can be long

            builder.Property(x => x.DeviceId)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(x => x.LastUpdated)
                   .IsRequired();

        }
    }
}
