using EaseClub.Domain.Memberships;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EaseClub.Infrastructure.Data.Configurations
{
    public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
    {
        public void Configure(EntityTypeBuilder<Enrollment> builder)
        {
            builder.ToTable("Enrollments");
            builder.HasKey(e => e.Id);

            builder.Property(e => e.ReadableId)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(e => e.Amount)
                .HasPrecision(18, 2);

            builder.Property(e => e.TotalPrice)
                .HasPrecision(18, 2);

            builder.Property(e => e.InstallmentsJson)
                .IsRequired();

            builder.HasIndex(e => e.MemberId);
            builder.HasIndex(e => e.ClubId);
            builder.HasIndex(e => e.Status);
            builder.HasIndex(e => e.FirstInvoiceId);
            builder.HasIndex(e => e.MembershipApplicationId);
        }
    }
}
