using EaseClub.Domain.MembershipPlans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Data.Configurations
{
    public class InstallmentTemplateConfiguration : IEntityTypeConfiguration<InstallmentTemplate>
    {
        public void Configure(EntityTypeBuilder<InstallmentTemplate> builder)
        {
            builder.HasKey(x => x.Id);

            // Map the List<Installment> as an Owned Collection
            builder.OwnsMany(x => x.Installments, a =>
            {
                a.ToTable("InstallmentTemplateItems"); // Stores them in a separate table
                a.WithOwner().HasForeignKey("InstallmentTemplateId"); // Shadow FK
                a.Property<int>("Id"); // Shadow Primary Key for DB only
                a.HasKey("Id");

                a.Property(x => x.PercentageOfAmount).HasPrecision(5, 2);
                a.Property(x => x.DueAfterDays).IsRequired();
                a.Property(x => x.OrderIndex).IsRequired();
            });

            builder.Navigation(x => x.Installments).HasField("_Installments").UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
