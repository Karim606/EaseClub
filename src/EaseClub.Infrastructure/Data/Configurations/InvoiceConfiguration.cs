using EaseClub.Domain.Payment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Data.Configurations
{
    public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
            builder.HasKey(i => i.Id);

            builder.Property(i => i.PayableId)
                .IsRequired();

            builder.Property(i => i.PayableType)
                .IsRequired();

            builder.Property(i => i.ClubId)
                .IsRequired();

            builder.Property(i => i.UserId)
                .IsRequired();

            builder.Property(i => i.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(i => i.DueDate)
                .IsRequired();

            builder.Property(i => i.Status)
                .IsRequired();

            // Transactions as one-to-many
            builder.HasMany(i => i.Transactions)
                .WithOne()
                .HasForeignKey(t => t.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(i => i.User)
                .WithMany()
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(i => i.Club)
                .WithMany()
                .HasForeignKey(i => i.ClubId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Navigation(i => i.Transactions).HasField("_Transactions").UsePropertyAccessMode(PropertyAccessMode.Field);

            // AuditableEntity properties mapping if needed
            builder.Property(i => i.CreatedAt).IsRequired();
            builder.Property(i => i.UpdatedAt).IsRequired(false);
        }
    }
}
