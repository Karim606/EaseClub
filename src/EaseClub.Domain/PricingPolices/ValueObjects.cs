using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.PricingPolices
{
    public record PricingContext(Dictionary<string, string?> Data);

    public record PricingResult(
        decimal BasePrice,
        decimal TotalPrice,
        List<AppliedPolicyDetail> AppliedPolicies);

    public record AppliedPolicyDetail(string Name, decimal Adjustment);
}
