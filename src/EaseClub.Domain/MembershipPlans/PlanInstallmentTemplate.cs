using EaseClub.Domain.Common.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.MembershipPlans
{
    public class PlanInstallmentTemplate
    {
        public Guid MembershipPlanId { get; set; }
        public MembershipPlan MembershipPlan { get; set; } = null!; // Navigation property

        public Guid InstallmentTemplateId { get; set; }
        public InstallmentTemplate InstallmentTemplate { get; set; } = null!; // Navigation property

        private PlanInstallmentTemplate() { } // EF
        private  PlanInstallmentTemplate(Guid membershipPlanId, Guid installmentTemplateId)
        {
            MembershipPlanId = membershipPlanId;
            InstallmentTemplateId = installmentTemplateId;
        }

        public static Result<PlanInstallmentTemplate> Create(Guid membershipPlanId, Guid installmentTemplateId)
        {
            if(membershipPlanId == Guid.Empty)
                return InstallmentTemplateErrors.MembershipGuidMustBeProvided;

            if(installmentTemplateId == Guid.Empty)
                return InstallmentTemplateErrors.InstallmentTemplateGuidMustBeProvided;

            return new PlanInstallmentTemplate(membershipPlanId, installmentTemplateId);
        }
    }
}
