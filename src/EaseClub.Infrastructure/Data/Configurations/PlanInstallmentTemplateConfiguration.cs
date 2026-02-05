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
    public class PlanInstallmentTemplateConfiguration : IEntityTypeConfiguration<PlanInstallmentTemplate>
    {
        public void Configure(EntityTypeBuilder<PlanInstallmentTemplate> builder)
        {
            builder.ToTable("PlanInstallmentTemplates");

            // Composite Key
            builder.HasKey(x => new { x.MembershipPlanId, x.InstallmentTemplateId });

            builder.HasOne(x => x.MembershipPlan)
                   .WithMany(x => x.InstallmentTemplates)
                   .HasForeignKey(x => x.MembershipPlanId);

            builder.HasOne(x => x.InstallmentTemplate)
                   .WithMany(x => x.MembershipPlans)
                   .HasForeignKey(x => x.InstallmentTemplateId);
        }
    }
}
