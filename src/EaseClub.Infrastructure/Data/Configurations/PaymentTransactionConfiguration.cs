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
    public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
    {
        public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.InvoiceId)
                .IsRequired();

            builder.Property(t => t.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(t => t.ExternalRef)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.Method)
                .IsRequired();

            builder.Property(t => t.Status)
                .IsRequired();

            builder.Property(t => t.CreatedAt)
                .IsRequired();

            builder.Property(t => t.CompletedAt)
                .IsRequired(false);

            builder.Property(t => t.FailureReason)
                .HasMaxLength(500)
                .IsRequired(false);

            // Optional: enforce unique ExternalRef per invoice
            builder.HasIndex(t => new { t.InvoiceId, t.ExternalRef })
                .IsUnique();
        }
    }
}
