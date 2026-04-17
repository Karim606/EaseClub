using EaseClub.Domain.Clubs;
using EaseClub.Domain.Member;
using EaseClub.Domain.Memberships;
using EaseClub.Domain.Payment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EaseClub.Infrastructure.Data.Configurations
{
    public class PendingEnrollmentConfiguration : IEntityTypeConfiguration<PendingEnrollment>
    {
        public void Configure(EntityTypeBuilder<PendingEnrollment> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.TotalPrice).HasColumnType("decimal(18,2)");
            builder.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            builder.Property(x => x.ReadableId).HasMaxLength(64).IsRequired();
            builder.Property(x => x.InstallmentsJson).IsRequired();
            builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
            builder.Property(x => x.Source).HasConversion<string>().HasMaxLength(32);

            builder.HasOne<MemberUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<Club>().WithMany().HasForeignKey(x => x.ClubId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.FirstInvoiceId);
            builder.HasIndex(x => x.MembershipApplicationId);
        }
    }
}
