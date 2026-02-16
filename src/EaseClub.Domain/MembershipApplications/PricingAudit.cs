using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipApplications.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.MembershipApplications
{
    public class PricingAudit:Entity
    {
        private PricingAudit() { }

        private PricingAudit(
            Guid id,
            Guid applicationId,
            Guid pricingPolicyId,
            decimal oldPrice,
            decimal newPrice):base(id)
        {
            ApplicationId = applicationId;
            PricingPolicyId = pricingPolicyId;
            OldPrice = oldPrice;
            NewPrice = newPrice;
            AppliedAt = DateTime.UtcNow;
        }

        public Guid ApplicationId { get; private set; }
        public Guid PricingPolicyId { get; private set; }
        public decimal OldPrice { get; private set; }
        public decimal NewPrice { get; private set; }
        public DateTime AppliedAt { get; private set; }

        public static Result<PricingAudit> Create(
            Guid id,
            Guid applicationId,
            Guid pricingPolicyId,
            decimal oldPrice,
            decimal newPrice)
        {
            if (applicationId == Guid.Empty)
                return PricingAuditErrors.ApplicationIdRequired;

            if (pricingPolicyId == Guid.Empty)
                return PricingAuditErrors.PricingPolicyIdRequired;

            if (oldPrice < 0)
                return PricingAuditErrors.InvalidOldPrice;

            if (newPrice < 0)
                return PricingAuditErrors.InvalidNewPrice;

            if (oldPrice == newPrice)
                return PricingAuditErrors.NoPriceChange;

            return new PricingAudit(
                id,
                applicationId,
                pricingPolicyId,
                oldPrice,
                newPrice);
        }
    }
}
